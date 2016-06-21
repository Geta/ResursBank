using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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


namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Business
{
    public class ResursCheckoutPaymentGateway : AbstractPaymentGateway, IPaymentOption
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(ResursCheckoutPaymentGateway));

        protected readonly LocalizationService _localizationService;
        public string CardNumber { get; set; }
        public string ResursPaymentMethod { get; set; }
        public string GovernmentId { get; set; }
        public decimal AmountForNewCard { get; set; }
        public decimal MinLimit { get; set; }
        public decimal MaxLimit { get; set; }

        public string CallbackUrl { get; set; }
        public string InvoiceDeliveryType { get; set; }

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

        public override bool ProcessPayment(Payment payment, ref string message)
        {
            try
            {
                Logger.Debug("Resurs checkout gateway. Processing Payment ....");
                if (VerifyConfiguration())
                {
                    var factory = ServiceLocator.Current.GetInstance<IResursBankServiceSettingFactory>();
                    var resursBankServices = factory.Init(ResursCredential);
                    // Check payment was processed or not.
                    bookPaymentResult bookPaymentResult = GetObjectFromCookie<bookPaymentResult>(ResursConstants.PaymentResultCookieName);

                    if (bookPaymentResult == null)
                    {
                        OrderForm orderForm = payment.Parent as OrderForm;
                        BookPaymentObject bookPaymentObject = new BookPaymentObject();
                        var resursBankPayment = payment as ResursBankPayment;
                        if (resursBankPayment != null)
                        {
                            // Get information of Customer from Billing Address of Order form
                            var billingAddress = orderForm.Parent.OrderAddresses.FirstOrDefault(x => x.Name == orderForm.BillingAddressId);
                            
                            bookPaymentObject.ExtendedCustomer = CreateExtendedCustomer(billingAddress);

                            bookPaymentObject.ExtendedCustomer.governmentId =
                                payment.GetStringValue(ResursConstants.GovernmentId, string.Empty);

                            //create paymentData
                            bookPaymentObject.PaymentData = new paymentData();
                            bookPaymentObject.PaymentData.paymentMethodId = payment.GetStringValue(ResursConstants.ResursBankPaymentType, string.Empty);
                            bookPaymentObject.PaymentData.customerIpAddress = HttpContext.Current.Request.UserHostAddress;
                            
                            //create paymentSpecification;
                            bookPaymentObject.PaymentSpec = CreatePaymentSpecification(orderForm);

                            bookPaymentObject.MapEntry = null;
                            
                            var _signing = new signing()
                            {
                                failUrl = payment.GetStringValue(ResursConstants.FailBackUrl, string.Empty),
                                forceSigning = false,
                                forceSigningSpecified = false,
                                successUrl = payment.GetStringValue(ResursConstants.SuccessfullUrl, string.Empty)
                            };

                            bookPaymentObject.Signing = _signing;
                            bookPaymentObject.CallbackUrl = !string.IsNullOrEmpty(resursBankPayment.CallBackUrl) ? resursBankPayment.CallBackUrl : "/";

                            //card info
                            cardData customerCard = null;
                            invoiceData invoice = null;
                            if (bookPaymentObject.PaymentData.paymentMethodId.Equals(ResursPaymentMethodType.CARD))
                            {
                                customerCard = new cardData();
                                customerCard.cardNumber = payment.GetStringValue(ResursConstants.CardNumber, string.Empty);
                            }
                            else if (bookPaymentObject.PaymentData.paymentMethodId.Equals(ResursPaymentMethodType.NEWCARD))
                            {
                                
                                customerCard = new cardData();
                                customerCard.cardNumber = "0000";
                                customerCard.amount = Decimal.Parse(payment.GetStringValue(ResursConstants.AmountForNewCard, string.Empty));
                                customerCard.amountSpecified = true;
                                bookPaymentObject.Signing.forceSigning = true;
                            }
                            else if (bookPaymentObject.PaymentData.paymentMethodId.Equals(ResursPaymentMethodType.INVOICE) || bookPaymentObject.PaymentData.finalizeIfBooked == true)
                            {
                                invoice = new invoiceData();
                                invoice.invoiceDate = DateTime.Now;
                                invoiceDeliveryTypeEnum dType;
                                var invoiceDeliveryType = payment.GetStringValue(ResursConstants.InvoiceDeliveryType, string.Empty);
                                if (!System.Enum.TryParse<invoiceDeliveryTypeEnum>(invoiceDeliveryType, true, out dType))
                                {
                                    dType = invoiceDeliveryTypeEnum.EMAIL;
                                }
                                invoice.invoiceDeliveryType = dType;
                            }
                            //card object
                            bookPaymentObject.Card = customerCard;
                            //Invoice data
                            bookPaymentObject.InvoiceData = invoice;

                            // booking payment to Resurs API
                            bookPaymentResult = resursBankServices.BookPayment(bookPaymentObject);
                            message = Newtonsoft.Json.JsonConvert.SerializeObject(bookPaymentResult);

                            // Booking succesfull
                            if (bookPaymentResult.bookPaymentStatus == bookPaymentStatus.BOOKED || bookPaymentResult.bookPaymentStatus == bookPaymentStatus.FINALIZED)
                            {
                                return true;
                            }
                            // Required signing
                            else if (bookPaymentResult.bookPaymentStatus == bookPaymentStatus.SIGNING)
                            {
                                // Save payment to Cookie for re-process.
                                SaveObjectToCookie(bookPaymentResult, ResursConstants.PaymentResultCookieName, new TimeSpan(0, 1, 0, 0));
                                HttpContext.Current.Response.Redirect(bookPaymentResult.signingUrl);
                                return false;
                            }
                            return false;
                        }
                        return false;
                    }
                    // Re-process for booking require signing.
                    else
                    {
                        // booking signed payment
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

        public Payment PreProcess(OrderForm orderForm)
        {
            if (orderForm == null) throw new ArgumentNullException("orderForm");

            //validate
            if (orderForm.Total > this.MaxLimit || orderForm.Total < this.MinLimit)
            {
                //not valid
                throw new Exception("total is not in limit from " + this.MinLimit + " to " + this.MaxLimit);
            }
           
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
            payment.SetMetaField(ResursConstants.GovernmentId, GovernmentId, false);
            payment.SetMetaField(ResursConstants.AmountForNewCard, AmountForNewCard, false);
            payment.SetMetaField(ResursConstants.CallBackUrl, CallbackUrl, false);
            payment.SetMetaField(ResursConstants.InvoiceDeliveryType, InvoiceDeliveryType, false);
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
                var resursBankServices = factory.Init(new ResursCredential(ConfigurationManager.AppSettings["ResursBank:UserName"],ConfigurationManager.AppSettings["ResursBank:Password"]));
                lstPaymentMethodsResponse = resursBankServices.GetPaymentMethods(lang, custType, amount);
                //Cache list payment methods for 1 day as Resurs recommended.
                EPiServer.CacheManager.Insert("GetListResursPaymentMethods", lstPaymentMethodsResponse, new CacheEvictionPolicy(null, new TimeSpan(1, 0, 0, 0), CacheTimeoutType.Absolute));
            }

            return lstPaymentMethodsResponse;
        }

        private paymentSpec CreatePaymentSpecification(OrderForm orderForm)
        {
            var paymentSpec = new paymentSpec();
            var specLines = orderForm.LineItems.Select(item => item.ToSpecLineItem()).ToList();
            if (specLines != null && specLines.Any())
            {
                specLine[] spLines = new specLine[specLines.Count];
                var i = 0;
                decimal totalAmount = 0;
                decimal totalVatAmount = 0;
                foreach (var specLine in specLines)
                {
                    var spLine = new specLine
                    {
                        id = specLine.Id,
                        artNo = specLine.ArtNo,
                        description = specLine.Description,
                        quantity = specLine.Quantity,
                        unitMeasure = specLine.UnitMeasure,
                        unitAmountWithoutVat = specLine.UnitAmountWithoutVat,
                        vatPct = specLine.VatPct,
                        totalVatAmount = specLine.TotalVatAmount,
                        totalAmount = specLine.TotalAmount
                    };
                    totalAmount += specLine.TotalAmount;
                    totalVatAmount += specLine.TotalVatAmount;
                    spLines[i] = spLine;
                    i++;
                }
                paymentSpec.specLines = spLines;
                paymentSpec.totalAmount = totalAmount;
                paymentSpec.totalVatAmount = totalVatAmount;
                paymentSpec.totalVatAmountSpecified = true;

            }

            return paymentSpec;
        }

        private extendedCustomer CreateExtendedCustomer(OrderAddress billingAddress)
        {
            var extendCustomer = new extendedCustomer();
            if (billingAddress != null)
            {
                extendCustomer.address = new address
                {
                    fullName = billingAddress.FirstName + " " + billingAddress.LastName,
                    firstName = billingAddress.FirstName,
                    lastName = billingAddress.LastName,
                    addressRow1 = billingAddress.Line1,
                    addressRow2 =
                        !string.IsNullOrEmpty(billingAddress.Line2) ? billingAddress.Line2 : billingAddress.Line1,
                    postalArea = billingAddress.PostalCode,
                    postalCode = billingAddress.PostalCode
                };
                countryCode cCode;
                if (!Enum.TryParse(billingAddress.CountryCode, true, out cCode))
                {
                    cCode = countryCode.SE;
                }
                extendCustomer.address.country = cCode;
                extendCustomer.phone = billingAddress.DaytimePhoneNumber ?? billingAddress.EveningPhoneNumber;
                extendCustomer.email = billingAddress.Email;
                extendCustomer.type = billingAddress.CountryCode.ToLower() == "swe" || billingAddress.CountryCode.ToLower() == "se" ? customerType.LEGAL : customerType.NATURAL;

            }

            return extendCustomer;
        }
    }
}
