using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using EPiServer.Framework.Localization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Geta.EPi.Commerce.Payments.Resurs.Checkout.Extensions;
using Geta.Resurs.Checkout;
using Geta.Resurs.Checkout.Model;
using Geta.Resurs.Checkout.SimplifiedShopFlowService;

using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Exceptions;
using Mediachase.Commerce.Orders.Search;
using Mediachase.Commerce.Plugins.Payment;
using Mediachase.Commerce.Website;


namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Bussiness
{
    public class ResursCheckoutPaymentGateway : AbstractPaymentGateway, IPaymentOption
    {

        private static readonly ILogger Logger = LogManager.GetLogger(typeof(ResursCheckoutPaymentGateway));

        protected readonly LocalizationService _localizationService;

        public ResursCheckoutPaymentGateway(LocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public ResursCheckoutPaymentGateway()
        {

        }

        private ResursCredential _resursCredential;
        internal ResursCredential ResursCredential
        {
            get
            {

                if (_resursCredential == null)
                {
                    _resursCredential = new ResursCredential(Settings[ResursConstants.UserName], Settings[ResursConstants.Password]);
                }
                Logger.Debug(string.Format("Active Resurs merchant id is {0}", Settings[ResursConstants.UserName]));
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
                    Cart cart = payment.Parent.Parent as Cart;
                    OrderForm orderForm = payment.Parent as OrderForm;


                    var factory = ServiceLocator.Current.GetInstance<IResursBankServiceSettingFactory>();
                    var resursBankServices = factory.Init(ResursCredential);

                    var resursBankPayment = payment as ResursBankPayment;
                    if (resursBankPayment != null)
                    {
                        // Get information of Customer from Billing Address of Order form
                        var billingAddress = orderForm.Parent.OrderAddresses.FirstOrDefault(x => x.Name == orderForm.BillingAddressId);
                        Customer customer = new Customer();
                        if (billingAddress != null)
                        {
                            customer = new Customer();
                            var address = new Address();
                            address.FullName = billingAddress.FirstName + " " + billingAddress.LastName;
                            address.FirstName = billingAddress.FirstName;
                            address.LastName = billingAddress.LastName;
                            address.AddressRow1 = billingAddress.Line1;
                            address.AddressRow2 = !string.IsNullOrEmpty(billingAddress.Line2) ? billingAddress.Line2 : billingAddress.Line1;
                            address.CountryCode = billingAddress.CountryCode;
                            address.PostalCode = billingAddress.PostalCode;
                            address.PostalArea = billingAddress.PostalCode;
                            customer.Address = address;
                            customer.Email = billingAddress.Email;
                            customer.Phone = billingAddress.DaytimePhoneNumber;
                            customer.GovernmentId = billingAddress.CountryCode.ToLower() == "se"
                                ? payment.GetStringValue(Geta.Epi.Commerce.Payments.Resurs.Checkout.ResursConstants.ResursBankPaymentMethod, string.Empty)
                                : string.Empty;
                            customer.Type = "NATURAL";// billingAddress.CountryCode.ToLower() == "se" ? "LEGAL" : "NATURAL";
                        }

                        resursBankPayment.Customer = customer;
                        resursBankPayment.ResursBankPaymentMethodId = payment.GetStringValue(ResursConstants.ResursBankPaymentMethod, string.Empty);
                        resursBankPayment.CustomerIpAddress = HttpContext.Current.Request.UserHostAddress;
                        resursBankPayment.SpecLines = orderForm.LineItems.Select(item => item.ToSpecLineItem()).ToList();

                        var successUrl = resursBankPayment.SuccessUrl;
                        var failUrl = !string.IsNullOrEmpty(resursBankPayment.FailUrl) ? resursBankPayment.FailUrl : "/";
                        var forceSigning = resursBankPayment.ForceSigning;
                        var callBackUrl = !string.IsNullOrEmpty(resursBankPayment.CallBackUrl) ? resursBankPayment.CallBackUrl : "/";

                        //if (resursBankPayment.BookingStatus == "Begin")
                        {
                            var result = resursBankServices.BookPayment(resursBankPayment.ResursBankPaymentMethodId, resursBankPayment.CustomerIpAddress, resursBankPayment.SpecLines, resursBankPayment.Customer, successUrl, failUrl, forceSigning, callBackUrl);
                            if (result.bookPaymentStatus == bookPaymentStatus.SIGNING)
                            {
                                resursBankPayment.BookingStatus = "Signing";
                                resursBankPayment.PaymentId = result.paymentId;
                                //TODO: send this url to redirect user to signing page
                                var signingUrl = result.signingUrl;
                                return true;
                            }
                        }
                        //else if (resursBankPayment.BookingStatus == "Signed")
                        //{
                        //    var result = resursBankServices.BookSignedPayment(resursBankPayment.PaymentId);
                        //    if (result.bookPaymentStatus == bookPaymentStatus.BOOKED)
                        //    {
                        //        resursBankPayment.BookingStatus = "Booked";
                        //        return true;
                        //    }
                        //}
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
                PaymentMethodId = PaymentMethodId,
                PaymentMethodName = "ResursBankCheckout",
                OrderFormId = orderForm.OrderFormId,
                OrderGroupId = orderForm.OrderGroupId,
                Amount = orderForm.Total,
                Status = PaymentStatus.Pending.ToString(),
                TransactionType = TransactionType.Authorization.ToString(),
                CardNumber = CardNumber

            };
            payment.SetMetaField("ResursBankPaymentMethod", "Invoice", false);
            return payment;
        }

        public bool PostProcess(OrderForm orderForm)
        {
            return true;
        }

        public List<PaymentMethodResponse> GetResursPaymentMethods(string lang, string custType, decimal amount)
        {
            var factory = ServiceLocator.Current.GetInstance<IResursBankServiceSettingFactory>();
            var resursBankServices = factory.Init(new ResursCredential(ConfigurationSettings.AppSettings["ResursBankUserName"], ConfigurationSettings.AppSettings["ResursBankUserNamePassword"]));
            return resursBankServices.GetPaymentMethods(lang, custType, amount);
        }

        public string CardNumber { get; set; }
    }
}
