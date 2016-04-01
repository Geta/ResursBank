using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Geta.Resurs.Checkout;
using Geta.Resurs.Checkout.Model;
using Geta.Resurs.Checkout.SimplifiedShopFlowService;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Exceptions;
using Mediachase.Commerce.Plugins.Payment;
using Mediachase.Commerce.Website;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Bussiness
{
    public class ResursCheckoutPaymentGateway : AbstractPaymentGateway, IPaymentOption
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(ResursCheckoutPaymentGateway));

        private ResursCredential _resursCredential;
        internal ResursCredential ResursCredential
        {
            get
            {
                if (_resursCredential == null)
                {
                    _resursCredential = new ResursCredential(Settings[ResursConstants.UserName], Settings[ResursConstants.Password]);
                }
                Logger.Debug(string.Format("Active Resurs merchant id is {0}", ResursConstants.UserName));
                return _resursCredential;
            }
        }

        public Guid PaymentMethodId { get; set; }

        public override bool ProcessPayment(Payment payment, ref string message)
        {
            try
            {
                Logger.Debug("Resurs checkout gateway. Processing Payment ....");
                if (VerifyConfiguration())
                {
                    var factory = ServiceLocator.Current.GetInstance<IResursBankServiceSettingFactory>();
                    var resursBankServices = factory.Init(ResursCredential);
                    var resursBankPayment = payment as ResursBankPayment;
                    if (resursBankPayment != null)
                    {
                        var paymentMethodId = resursBankPayment.ResursBankPaymentMethodId;
                        var customerIpAddress = resursBankPayment.CustomerIpAddress;
                        var specLines = resursBankPayment.SpecLines;
                        var customer = resursBankPayment.Customer;
                        var successUrl = resursBankPayment.SuccessUrl;
                        var failUrl = resursBankPayment.FailUrl;
                        var forceSigning = resursBankPayment.ForceSigning;
                        var callBackUrl = resursBankPayment.CallBackUrl;

                        if (resursBankPayment.BookingStatus == "Begin")
                        {
                            var result = resursBankServices.BookPayment(paymentMethodId, customerIpAddress, specLines, customer, successUrl,
                            failUrl, forceSigning, callBackUrl);
                            if (result.bookPaymentStatus == bookPaymentStatus.SIGNING)
                            {
                                resursBankPayment.BookingStatus = "Signing";
                                resursBankPayment.PaymentId = result.paymentId;
                                //TODO: send this url to redirect user to signing page
                                var signingUrl = result.signingUrl;
                                return true;
                            }
                        }
                        else if (resursBankPayment.BookingStatus == "Signed")
                        {
                            var result = resursBankServices.BookSignedPayment(resursBankPayment.PaymentId);
                            if (result.bookPaymentStatus == bookPaymentStatus.BOOKED)
                            {
                                resursBankPayment.BookingStatus = "Booked";
                                return true;
                            }
                        }
                    }
                }

            }
            catch (Exception exception)
            {
                Logger.Error("Process payment failed with error: " + exception.Message, exception);
                throw;
            }
            return true;
        }

        private bool VerifyConfiguration()
        {
            if (string.IsNullOrEmpty(Settings[ResursConstants.UserName]))
            {
                throw new PaymentException(PaymentException.ErrorType.ConfigurationError, "",
                                           "Payment configuration is not valid. Missing payment provider username.");
            }

            if (string.IsNullOrEmpty(Settings[ResursConstants.Password]))
            {
                throw new PaymentException(PaymentException.ErrorType.ConfigurationError, "",
                                           "Payment configuration is not valid. Missing payment provider password.");
            }
            Logger.Debug("Payment method configuuration verified.");
            return true;
        }


        public bool ValidateData()
        {
            return true;
        }

        public Mediachase.Commerce.Orders.Payment PreProcess(OrderForm orderForm)
        {
            if (orderForm == null) throw new ArgumentNullException("orderForm");

            if (!ValidateData())
                return null;

            if (orderForm == null) throw new ArgumentNullException("orderForm");

            if (!ValidateData())
                return null;

            var payment = new ResursBankPayment()
            {
                // Hard code PaymentMethodId from Ecommerce Manager
                PaymentMethodId = PaymentMethodId,
                PaymentMethodName = "ResursBankCheckout",
                OrderFormId = orderForm.OrderFormId,
                OrderGroupId = orderForm.OrderGroupId,
                Amount = orderForm.Total,
                Status = PaymentStatus.Pending.ToString(),
                TransactionType = TransactionType.Authorization.ToString()
            };

            return payment;

            
        }

        public bool PostProcess(OrderForm orderForm)
        {
            var card = orderForm.Payments.ToArray().FirstOrDefault(x => x.PaymentType == PaymentType.CreditCard);
            if (card == null)
                return false;

            card.Status = PaymentStatus.Pending.ToString();
            card.AcceptChanges();
            return true;
        }

        public List<PaymentMethodResponse> GetResursPaymentMethods(string lang, string custType, decimal amount)
        {
            var factory = ServiceLocator.Current.GetInstance<IResursBankServiceSettingFactory>();
            var resursBankServices = factory.Init(new ResursCredential(ConfigurationSettings.AppSettings["ResursBankUserName"], ConfigurationSettings.AppSettings["ResursBankUserNamePassword"]));
            return resursBankServices.GetPaymentMethods(lang, custType, amount);
        }
    }
}
