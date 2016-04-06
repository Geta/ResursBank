using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using EPiServer.Framework.Cache;
using EPiServer.Framework.Localization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Geta.EPi.Commerce.Payments.Resurs.Checkout.Extensions;
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

        public string CallBackUrlWhenFail { get; set; }

        public string SuccessUrl { get; set; }

        public string CardNumber { get; set; }

        public string ResursPaymentMethod { get; set; }

        public string GovernmentId { get; set; }

        public override bool ProcessPayment(Payment payment, ref string message)
        {
            try
            {

                Logger.Debug("Resurs checkout gateway. Processing Payment ....");
                if (VerifyConfiguration())
                {
                    var factory = ServiceLocator.Current.GetInstance<IResursBankServiceSettingFactory>();
                    var resursBankServices = factory.Init(ResursCredential);
                    bookPaymentResult bookPaymentResult =
                        GetObjectFromCookie<bookPaymentResult>(ResursConstants.PaymentResultCookieName);
                    if (bookPaymentResult == null)
                    {
                        Cart cart = payment.Parent.Parent as Cart;
                        OrderForm orderForm = payment.Parent as OrderForm;


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
                                customer.Phone = !string.IsNullOrEmpty(billingAddress.DaytimePhoneNumber) ? billingAddress.DaytimePhoneNumber : "+4797674852"; //hard code
                                customer.GovernmentId = billingAddress.CountryCode.ToLower() == "swe"
                                   ? payment.GetStringValue(ResursConstants.GovernmentId, string.Empty)
                                   : "010986-14741";
                                customer.Type = "NATURAL";// billingAddress.CountryCode.ToLower() == "se" ? "LEGAL" : "NATURAL";
                            }

                            resursBankPayment.Customer = customer;
                            resursBankPayment.ResursBankPaymentMethodId = payment.GetStringValue(ResursConstants.ResursBankPaymentMethod, string.Empty);
                            var resurPaymentType = payment.GetStringValue(ResursConstants.ResursBankPaymentType, string.Empty);

                            resursBankPayment.CustomerIpAddress = HttpContext.Current.Request.UserHostAddress;
                            resursBankPayment.SpecLines = orderForm.LineItems.Select(item => item.ToSpecLineItem()).ToList();

                            var successUrl = payment.GetStringValue(ResursConstants.SuccessfullUrl, string.Empty);
                            var failUrl = payment.GetStringValue(ResursConstants.FailBackUrl, string.Empty);
                            resursBankPayment.ForceSigning = false;
                            var callBackUrl = !string.IsNullOrEmpty(resursBankPayment.CallBackUrl) ? resursBankPayment.CallBackUrl : "/";

                            var card = new Card(CardNumber);

                            bookPaymentResult = resursBankServices.BookPayment(resurPaymentType, resursBankPayment.CustomerIpAddress, resursBankPayment.SpecLines, resursBankPayment.Customer, card, successUrl, failUrl, resursBankPayment.ForceSigning, callBackUrl);
                            message = Newtonsoft.Json.JsonConvert.SerializeObject(bookPaymentResult);

                            if (bookPaymentResult.bookPaymentStatus == bookPaymentStatus.BOOKED ||
                                bookPaymentResult.bookPaymentStatus == bookPaymentStatus.FINALIZED)
                            {
                                return true;
                            }
                            else if (bookPaymentResult.bookPaymentStatus == bookPaymentStatus.SIGNING)
                            {
                                resursBankPayment.BookingStatus = "Signing";
                                resursBankPayment.PaymentId = bookPaymentResult.paymentId;
                                SaveObjectToCookie(bookPaymentResult, ResursConstants.PaymentResultCookieName, new TimeSpan(0, 1, 0, 0));

                                //TODO: send this url to redirect user to signing page
                                HttpContext.Current.Response.Redirect(bookPaymentResult.signingUrl);
                                return false;
                            }
                            return false;
                        }
                        return false;
                    }
                    else
                    {
                        bookPaymentResult = resursBankServices.BookSignedPayment(bookPaymentResult.paymentId);
                        SaveObjectToCookie(null, ResursConstants.PaymentResultCookieName, new TimeSpan(0, 1, 0, 0));
                        message = Newtonsoft.Json.JsonConvert.SerializeObject(bookPaymentResult);
                        if (bookPaymentResult.bookPaymentStatus == bookPaymentStatus.BOOKED || bookPaymentResult.bookPaymentStatus == bookPaymentStatus.FINALIZED)
                        {
                            return true;
                        }
                        else
                        {
                            SaveObjectToCookie(null, ResursConstants.PaymentResultCookieName, new TimeSpan(0, 1, 0, 0));
                            return false;
                        }

                    }
                }

            }
            catch (Exception exception)
            {
                Logger.Error("Process payment failed with error: " + exception.Message, exception);
                message = exception.Message;
                throw;
            }
            return true;
        }

        private void SaveObjectToCookie(Object obj, string keyName, TimeSpan timeSpan)
        {
            string myObjectJson = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            var cookie = new HttpCookie(keyName, myObjectJson)
            {
                Expires = DateTime.Now.Add(timeSpan)
            };
            if (HttpContext.Current.Response.Cookies[keyName] == null)
            {
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            else
            {
                HttpContext.Current.Response.Cookies.Set(cookie);
            }

        }

        private T GetObjectFromCookie<T>(string keyName)
        {
            if (HttpContext.Current.Request.Cookies[keyName] != null)
            {
                var s = HttpContext.Current.Server.UrlDecode(HttpContext.Current.Request.Cookies[keyName].Value);
                return !string.IsNullOrEmpty(s) ? Newtonsoft.Json.JsonConvert.DeserializeObject<T>(s) : default(T);
            }
            return default(T);
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
            };
            payment.SetMetaField(ResursConstants.ResursBankPaymentType, ResursPaymentMethod, false);
            payment.SetMetaField(ResursConstants.CardNumber, CardNumber, false);
            payment.SetMetaField(ResursConstants.FailBackUrl, CallBackUrlWhenFail, false);
            payment.SetMetaField(ResursConstants.SuccessfullUrl, SuccessUrl, false);
            return payment;
        }

        public bool PostProcess(OrderForm orderForm)
        {
            return true;
        }

        public List<PaymentMethodResponse> GetResursPaymentMethods(string lang, string custType, decimal amount)
        {
            List<PaymentMethodResponse> lstPaymentMethodsResponse = EPiServer.CacheManager.Get("GetListResursPaymentMethods") as List<PaymentMethodResponse>;
            if (lstPaymentMethodsResponse == null || !lstPaymentMethodsResponse.Any())
            {
                var factory = ServiceLocator.Current.GetInstance<IResursBankServiceSettingFactory>();
                var resursBankServices = factory.Init(new ResursCredential(ConfigurationSettings.AppSettings["ResursBankUserName"], ConfigurationSettings.AppSettings["ResursBankUserNamePassword"]));
                lstPaymentMethodsResponse = resursBankServices.GetPaymentMethods(lang, custType, amount);
                //Cache list payment methods for 1 day as Resurs recommended.
                EPiServer.CacheManager.Insert("GetListResursPaymentMethods", lstPaymentMethodsResponse, new CacheEvictionPolicy(null, new TimeSpan(1, 0, 0, 0), CacheTimeoutType.Absolute));
            }
            return lstPaymentMethodsResponse;
        }

    }
}
