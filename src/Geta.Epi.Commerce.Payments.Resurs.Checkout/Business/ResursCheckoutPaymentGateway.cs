using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Web;
using EPiServer.Commerce.Order;
using EPiServer.Framework.Cache;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Geta.Epi.Commerce.Payments.Resurs.Checkout.Extensions;
using Geta.EPi.Commerce.Payments.Resurs.Checkout.Extensions;
using Geta.Resurs.Checkout;
using Geta.Resurs.Checkout.Model;
using Geta.Resurs.Checkout.SimplifiedShopFlowService;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Exceptions;
using Mediachase.Commerce.Plugins.Payment;


namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Business
{
    public class ResursCheckoutPaymentGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(ResursCheckoutPaymentGateway));

        private readonly IOrderFormCalculator _orderFormCalculator;
        private readonly IMarketService _marketService;
        private readonly IResursBankRedirectSettings _redirectSettings;

        public IOrderGroup OrderGroup { get; set; }

        public string CardNumber { get; set; }
        public string ResursPaymentMethod { get; set; }
        public string GovernmentId { get; set; }
        public decimal AmountForNewCard { get; set; }
        public decimal MinLimit { get; set; }
        public decimal MaxLimit { get; set; }
        
        public string InvoiceDeliveryType { get; set; }

        public ResursCheckoutPaymentGateway()
        {
            _orderFormCalculator = ServiceLocator.Current.GetInstance<IOrderFormCalculator>();
            _marketService = ServiceLocator.Current.GetInstance<IMarketService>();
            _redirectSettings = ServiceLocator.Current.GetInstance<IResursBankRedirectSettings>();
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
                Logger.Debug($"Active Resurs merchant id is {Settings[ResursConstants.UserName]}");
                return _resursCredential;
            }
        }

        internal bool IncludeShipping => bool.TryParse(Settings[ResursConstants.IncludeShippingCost], out bool result) && result;

        public Guid PaymentMethodId { get; set; }

        public virtual bool ProcessPayment(IPayment payment, ref string message)
        {
            return ProcessPayment(OrderGroup, payment).IsSuccessful;
        }

        public override bool ProcessPayment(Payment payment, ref string message)
        {
            OrderGroup = payment.Parent.Parent;

            return ProcessPayment(OrderGroup, payment).IsSuccessful;
        }

        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
        {
            string message;

            try
	        {
		        Logger.Debug("Resurs checkout gateway. Processing Payment ....");
		        if (VerifyConfiguration())
		        {
			        var factory = ServiceLocator.Current.GetInstance<IResursBankServiceSettingFactory>();
			        var resursBankServices = factory.Init(ResursCredential);
			        // Check payment was processed or not.
			        var bookPaymentResult =
				        GetObjectFromCookie<bookPaymentResult>(ResursConstants.PaymentResultCookieName);

			        if (bookPaymentResult == null)
			        {
				        var bookPaymentObject = new BookPaymentObject();
			            if (!(payment is ResursBankPayment resursBankPayment))
			            {
			                return PaymentProcessingResult.CreateUnsuccessfulResult($"Payment is not of type {nameof(ResursBankPayment)}");
			            }

			            // Get information of Customer from Billing Address of Order form
			            var billingAddress = payment.BillingAddress;

			            bookPaymentObject.ExtendedCustomer = CreateExtendedCustomer(billingAddress, payment.Properties[ResursConstants.GovernmentId] as string ?? string.Empty);
			            bookPaymentObject.PaymentData = CreatePaymentData(payment);
			            bookPaymentObject.PaymentSpec = CreatePaymentSpecification(orderGroup, orderGroup.GetFirstForm(), IncludeShipping);
			            bookPaymentObject.MapEntry = null;
			            bookPaymentObject.Signing = CreateSigning(payment);
			            bookPaymentObject.CallbackUrl = !string.IsNullOrEmpty(resursBankPayment.CallBackUrl)
			                ? resursBankPayment.CallBackUrl
			                : null;

			            bookPaymentObject.Card = CreateCustomerCard(payment, bookPaymentObject);
			            bookPaymentObject.InvoiceData = CreateInvoiceData(payment, bookPaymentObject);

			            // booking payment to Resurs API
			            bookPaymentResult = resursBankServices.BookPayment(bookPaymentObject);
			            message = Newtonsoft.Json.JsonConvert.SerializeObject(bookPaymentResult);

                        // Save payment Id generated by Resurs
			            payment.Properties[ResursConstants.ResursPaymentId] = bookPaymentResult.paymentId;

                        // Booking succesfull
                        switch (bookPaymentResult.bookPaymentStatus)
                        {
                            case bookPaymentStatus.FROZEN:
                                payment.Properties[ResursConstants.PaymentFreezeStatus] = true;
                                return PaymentProcessingResult.CreateSuccessfulResult(message);
                            case bookPaymentStatus.BOOKED:
                            case bookPaymentStatus.FINALIZED:
                                payment.Properties[ResursConstants.PaymentFreezeStatus] = false;
                                return PaymentProcessingResult.CreateSuccessfulResult(message);
                            case bookPaymentStatus.SIGNING:
                                // Save payment to Cookie for re-process.
                                SaveObjectToCookie(bookPaymentResult, ResursConstants.PaymentResultCookieName, new TimeSpan(0, 1, 0, 0));
                                return PaymentProcessingResult.CreateSuccessfulResult(message, bookPaymentResult.signingUrl);
                            case bookPaymentStatus.DENIED:
                                message = "Booking of payment was denied.";
                                return PaymentProcessingResult.CreateUnsuccessfulResult(message);
                            default:
                                return PaymentProcessingResult.CreateUnsuccessfulResult(message);
                        }
                    }
                    
                    // Re-process for bookings that require signing.
		            // booking signed payment
		            bookPaymentResult = resursBankServices.BookSignedPayment(bookPaymentResult.paymentId);

		            // Clear cookie / set cookie to null
		            SaveObjectToCookie(null, ResursConstants.PaymentResultCookieName, new TimeSpan(0, 1, 0, 0));

		            message = Newtonsoft.Json.JsonConvert.SerializeObject(bookPaymentResult);

		            switch (bookPaymentResult.bookPaymentStatus)
		            {
		                case bookPaymentStatus.BOOKED:
		                case bookPaymentStatus.FINALIZED:
		                    payment.Properties[ResursConstants.PaymentFreezeStatus] = false;
		                    return PaymentProcessingResult.CreateSuccessfulResult(message);
		                case bookPaymentStatus.FROZEN:
		                    payment.Properties[ResursConstants.PaymentFreezeStatus] = true;
		                    return PaymentProcessingResult.CreateSuccessfulResult(message);
		                default:
		                    return PaymentProcessingResult.CreateUnsuccessfulResult(message);
		            }
		        }
            }
            catch (FaultException<ECommerceError> exception)
	        {
				Logger.Error("Process payment failed with error: " + exception.Message, exception);
				Logger.Error(exception.Detail.errorTypeDescription);
				message = exception.Detail.userErrorMessage;

				return PaymentProcessingResult.CreateUnsuccessfulResult(message);
			}
            catch (Exception exception)
            {
				Logger.Error("Process payment failed with error: " + exception.Message, exception);
                message = exception.Message;

                return PaymentProcessingResult.CreateUnsuccessfulResult(message);
            }

            return PaymentProcessingResult.CreateUnsuccessfulResult("If you see this text, it means that something went very wrong, it shouldn't happen, ever.");
        }

        private cardData CreateCustomerCard(IPayment payment, BookPaymentObject bookPaymentObject)
        {
            var paymentMethodId = bookPaymentObject.PaymentData.paymentMethodId;

            if (paymentMethodId.Equals(ResursPaymentMethodType.CARD) ||
                paymentMethodId.Equals(ResursPaymentMethodType.ACCOUNT))
            {
                return new cardData
                {
                    cardNumber = payment.Properties[ResursConstants.CardNumber] as string ?? string.Empty
                };
            }
            
            if (paymentMethodId.Equals(ResursPaymentMethodType.NEWCARD) ||
                paymentMethodId.Equals(ResursPaymentMethodType.NEWACCOUNT))
            {
                bookPaymentObject.Signing.forceSigning = true;

                if (!decimal.TryParse(payment.Properties[ResursConstants.AmountForNewCard] as string, out var amount))
                {
                    amount = default(decimal);
                }

                return new cardData
                {
                    cardNumber = "0000",
                    amount = amount,
                    amountSpecified = true
                };
            }
            return null;
        }

        private invoiceData CreateInvoiceData(IPayment payment, BookPaymentObject bookPaymentObject)
        {
            if (bookPaymentObject.PaymentData.paymentMethodId.Equals(ResursPaymentMethodType.INVOICE) ||
                bookPaymentObject.PaymentData.finalizeIfBooked == true)
            {
                return new invoiceData
                {
                    invoiceDate = DateTime.Now,
                    invoiceDeliveryType = GetInvoiceDeliveryType(payment)
                };
            }

            return null;
        }

        private invoiceDeliveryTypeEnum GetInvoiceDeliveryType(IPayment payment)
        {
            var invoiceDeliveryType = payment.Properties[ResursConstants.InvoiceDeliveryType] as string ?? string.Empty;
            if (!Enum.TryParse(invoiceDeliveryType, true, out invoiceDeliveryTypeEnum dType))
            {
                dType = invoiceDeliveryTypeEnum.EMAIL;
            }
            return dType;
        }

        private signing CreateSigning(IPayment payment)
        {
            return new signing
            {
                failUrl = payment.Properties[ResursConstants.FailBackUrl] as string ?? string.Empty,
                forceSigning = false,
                forceSigningSpecified = false,
                successUrl = payment.Properties[ResursConstants.SuccessfullUrl] as string ?? string.Empty
            };
        }

        private paymentData CreatePaymentData(IPayment payment)
        {
            return new paymentData
            {
                paymentMethodId = payment.Properties[ResursConstants.ResursBankPaymentType] as string ?? string.Empty,
                customerIpAddress = HttpContext.Current.Request.UserHostAddress
            };
        }

        private void SaveObjectToCookie(Object obj, string keyName, TimeSpan timeSpan)
        {
            var myObjectJson = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
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

        public IPayment PreProcess(IOrderGroup orderGroup, IOrderForm orderForm)
        {
            if (orderForm == null) throw new ArgumentNullException(nameof(orderForm));

            var market = _marketService.GetMarket(orderGroup.MarketId);
            var totals = _orderFormCalculator.GetOrderFormTotals(orderForm, market, orderGroup.Currency);

            //validate
            if (totals.Total > MaxLimit || totals.Total < MinLimit)
            {
                //not valid
                throw new Exception($"total is not in limit from {MinLimit} to {MaxLimit}");
            }
           
            if (!ValidateData())
                return null;

            if (orderForm == null) throw new ArgumentNullException(nameof(orderForm));

            if (!ValidateData())
                return null;

            var payment = new ResursBankPayment
            {
                PaymentMethodId = PaymentMethodId,
                PaymentMethodName = "ResursBankCheckout",
                OrderFormId = orderForm.OrderFormId,
                OrderGroupId = orderGroup.OrderLink.OrderGroupId,
                Amount = totals.Total,
                Status = PaymentStatus.Pending.ToString(),
                TransactionType = TransactionType.Authorization.ToString(),
            };

            payment.SetMetaField(ResursConstants.ResursBankPaymentType, ResursPaymentMethod, false);
            payment.SetMetaField(ResursConstants.CardNumber, CardNumber, false);
            payment.SetMetaField(ResursConstants.CallBackUrl, _redirectSettings.CallbackUrl, false);
            payment.SetMetaField(ResursConstants.FailBackUrl, _redirectSettings.FailureCallbackUrl, false);
            payment.SetMetaField(ResursConstants.SuccessfullUrl, _redirectSettings.SuccessRedirectUrl, false);
            payment.SetMetaField(ResursConstants.GovernmentId, GovernmentId, false);
            payment.SetMetaField(ResursConstants.AmountForNewCard, AmountForNewCard, false);
            payment.SetMetaField(ResursConstants.InvoiceDeliveryType, InvoiceDeliveryType, false);

            return payment;
        }

        public bool PostProcess(IOrderForm orderForm)
        {
            var resursPayment =
                orderForm.Payments.FirstOrDefault(x => x.PaymentMethodName.Equals("ResursBankCheckout"));

            // Check if payment frozen, if so set payment pending
            if (resursPayment == null)
            {
                return true;
            }

            if (resursPayment.GetResursFreezeStatus())
            {
                resursPayment.Status = PaymentStatus.Pending.ToString();
            }

            return true;
        }

        public List<PaymentMethodResponse> GetResursPaymentMethods(string lang, string custType, decimal amount)
        {
            var lstPaymentMethodsResponse = EPiServer.CacheManager.Get("GetListResursPaymentMethods") as List<PaymentMethodResponse>;
            if (lstPaymentMethodsResponse == null || !lstPaymentMethodsResponse.Any())
            {
                var factory = ServiceLocator.Current.GetInstance<IResursBankServiceSettingFactory>();
                var resursBankServices = factory.Init(new ResursCredential(ConfigurationManager.AppSettings["ResursBank:UserName"],ConfigurationManager.AppSettings["ResursBank:Password"]));
                lstPaymentMethodsResponse = resursBankServices.GetPaymentMethods(lang, custType, amount);
                //Cache list payment methods for 1 day as Resurs recommended.
                EPiServer.CacheManager.Insert("GetListResursPaymentMethods", lstPaymentMethodsResponse, new CacheEvictionPolicy(new TimeSpan(1, 0, 0, 0), CacheTimeoutType.Absolute));
            }

            return lstPaymentMethodsResponse;
        }

        private paymentSpec CreatePaymentSpecification(IOrderGroup orderGroup, IOrderForm orderForm, bool includeShipping = false)
        {
            var paymentSpec = new paymentSpec();

            var specLines = orderForm.GetAllLineItems().Select(item => item.ToSpecLineItem(orderGroup, orderForm)).ToList();
            if (!specLines.Any())
            {
                return paymentSpec;
            }

            var market = _marketService.GetMarket(orderGroup.MarketId);
            var totals = _orderFormCalculator.GetOrderFormTotals(orderForm, market, orderGroup.Currency);

            var itemCount = includeShipping && totals.ShippingTotal > 0 ? specLines.Count + 1 : specLines.Count;

            var spLines = new specLine[itemCount];

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

            if (includeShipping && totals.ShippingTotal > 0)
            {
                var spLine = new specLine
                {
                    id = "Shipping",
                    artNo = "Shipping",
                    unitMeasure = UnitMeasureType.ST,
                    description = "Frakt",
                    totalAmount = totals.ShippingTotal,
                    unitAmountWithoutVat = totals.ShippingTotal,
                    quantity = 1
                };

                totalAmount += spLine.totalAmount;
                totalVatAmount += spLine.totalVatAmount;

                spLines[specLines.Count] = spLine;
            }

            paymentSpec.specLines = spLines;
            paymentSpec.totalAmount = totalAmount;
            paymentSpec.totalVatAmount = totalVatAmount;
            paymentSpec.totalVatAmountSpecified = true;

            return paymentSpec;
        }

        private extendedCustomer CreateExtendedCustomer(IOrderAddress billingAddress, string governmentId)
        {
            var extendCustomer = new extendedCustomer
            {
                governmentId = governmentId
            };

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

                var billingCountryCode = GetBillingCountryCode(billingAddress.CountryCode);

                if (!Enum.TryParse(billingCountryCode, true, out countryCode cCode))
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

        private string GetBillingCountryCode(string billingCountryCode)
        {
            if (!ServiceLocator.Current.TryGetExistingInstance(
                out ICountryCodeDictionary countryCodeDictionary))
            {
                return billingCountryCode;
            }

            var countryMap = countryCodeDictionary.GetCountryMap();
            return countryMap.ContainsKey(billingCountryCode) ? countryMap[billingCountryCode] : billingCountryCode;
        }

        
    }
}
