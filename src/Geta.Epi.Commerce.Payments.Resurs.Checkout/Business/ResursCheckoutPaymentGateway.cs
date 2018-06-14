﻿using System;
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
                if (!VerifyConfiguration())
                {
                    return PaymentProcessingResult.CreateUnsuccessfulResult(ResursCheckoutPaymentErrors.Configuration);
                }

	            if (!payment.PaymentMethodName.Equals(ResursConstants.ResursBankPaymentMethod))
                {
                    return PaymentProcessingResult.CreateUnsuccessfulResult(ResursCheckoutPaymentErrors.PaymentType);
                }

                var factory = ServiceLocator.Current.GetInstance<IResursBankServiceSettingFactory>();
			    var resursBankServices = factory.Init(ResursCredential);

                // Check if we already have a ResursPaymentId
                var resursPaymentId = payment.GetResursPaymentId();
                if (string.IsNullOrWhiteSpace(resursPaymentId))
                {
                    // New payment, call bookPayment
                    var bookPaymentObject = CreateBookPaymentObject(orderGroup, payment);

                    // booking payment to Resurs API
                    var bookPaymentResult = resursBankServices.BookPayment(bookPaymentObject);

                    // Save payment Id generated by Resurs
                    payment.Properties[ResursConstants.ResursPaymentId] = bookPaymentResult.paymentId;

                    // Booking succesfull
                    return HandleBookPaymentResult(payment, bookPaymentResult);
                }
                else
                {
                    // Existing payment, check signing result
                    return ReProcessBooking(resursBankServices, payment, resursPaymentId);
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

                return PaymentProcessingResult.CreateUnsuccessfulResult(ResursCheckoutPaymentErrors.Generic);
            }
        }

        public string CreatePreferredPaymentId()
        {
            return Guid.NewGuid().ToString().ToLower();
        }

        private BookPaymentObject CreateBookPaymentObject(IOrderGroup orderGroup, IPayment payment)
        {
            // Get information of Customer from Billing Address of Order form
            var billingAddress = payment.BillingAddress;
            var preferredPaymentId = CreatePreferredPaymentId();
            var bookPaymentObject = new BookPaymentObject
            {
                ExtendedCustomer = CreateExtendedCustomer(billingAddress, payment.Properties[ResursConstants.GovernmentId] as string ?? string.Empty),
                PaymentData = CreatePaymentData(payment, preferredPaymentId),
                PaymentSpec = CreatePaymentSpecification(orderGroup, orderGroup.GetFirstForm(), IncludeShipping),
                MapEntry = null,
                Signing = CreateSigning(payment, preferredPaymentId),
                CallbackUrl = null
            };
            bookPaymentObject.Card = CreateCustomerCard(payment, bookPaymentObject);
            bookPaymentObject.InvoiceData = CreateInvoiceData(payment, bookPaymentObject);

            return bookPaymentObject;
        }

        private PaymentProcessingResult HandleBookPaymentResult(IPayment payment, bookPaymentResult bookPaymentResult)
        {
            switch (bookPaymentResult.bookPaymentStatus)
            {
                case bookPaymentStatus.FROZEN:
                    payment.Properties[ResursConstants.PaymentFreezeStatus] = true;
                    return PaymentProcessingResult.CreateSuccessfulResult(string.Empty);
                case bookPaymentStatus.BOOKED:
                case bookPaymentStatus.FINALIZED:
                    payment.Properties[ResursConstants.PaymentFreezeStatus] = false;
                    return PaymentProcessingResult.CreateSuccessfulResult(string.Empty);
                case bookPaymentStatus.SIGNING:
                    return PaymentProcessingResult.CreateSuccessfulResult(string.Empty, bookPaymentResult.signingUrl);
                case bookPaymentStatus.DENIED:
                    return PaymentProcessingResult.CreateUnsuccessfulResult(ResursCheckoutPaymentErrors.PaymentDenied);
                default:
                    return PaymentProcessingResult.CreateUnsuccessfulResult(ResursCheckoutPaymentErrors.UnknownPaymentStatus);
            }
        }

        /// <summary>
        /// Re-process for bookings that require signing.
        /// </summary>
        private PaymentProcessingResult ReProcessBooking(
            ResursBankServiceClient resursBankServices,
            IPayment payment,
            string resursPaymentId)
        {
            // Try to book the signed payment
            var bookPaymentResult = resursBankServices.BookSignedPayment(resursPaymentId);

            return HandleBookPaymentResult(payment, bookPaymentResult);
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

        private signing CreateSigning(IPayment payment, string preferredPaymentId)
        {
            //TODO querystring
            return new signing
            {
                failUrl = $"{_redirectSettings.SigningFailedUrl}?paymentId={preferredPaymentId}",
                forceSigning = false,
                forceSigningSpecified = false,
                successUrl = $"{_redirectSettings.SigningSuccessUrl}?paymentId={preferredPaymentId}"
            };
        }

        private paymentData CreatePaymentData(IPayment payment, string preferredPaymentId)
        {
            return new paymentData
            {
                preferredId = preferredPaymentId,
                paymentMethodId = payment.Properties[ResursConstants.ResursBankPaymentType] as string ?? string.Empty,
                customerIpAddress = HttpContext.Current.Request.UserHostAddress
            };
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

            Logger.Debug("Payment method configuration verified.");
            return true;
        }


        public bool ValidateData()
        {
            return true;
        }

        //public IPayment PreProcess(IOrderGroup orderGroup, IOrderForm orderForm)
        //{
        //    if (orderForm == null) throw new ArgumentNullException(nameof(orderForm));

        //    var market = _marketService.GetMarket(orderGroup.MarketId);
        //    var totals = _orderFormCalculator.GetOrderFormTotals(orderForm, market, orderGroup.Currency);

        //    //validate
        //    if (totals.Total > MaxLimit || totals.Total < MinLimit)
        //    {
        //        //not valid
        //        throw new Exception($"total is not in limit from {MinLimit} to {MaxLimit}");
        //    }
           
        //    if (string.IsNullOrWhiteSpace(_redirectSettings.SuccessRedirectUrl) || 
        //        string.IsNullOrWhiteSpace(_redirectSettings.FailureCallbackUrl))
        //    {
        //        throw new Exception($"Please configure IResursbankRedirectSettings");
        //    }

        //    if (orderForm == null) throw new ArgumentNullException(nameof(orderForm));

        //    var payment = new ResursBankPayment
        //    {
        //        PaymentMethodId = PaymentMethodId,
        //        PaymentMethodName = "ResursBankCheckout",
        //        OrderFormId = orderForm.OrderFormId,
        //        OrderGroupId = orderGroup.OrderLink.OrderGroupId,
        //        Amount = totals.Total,
        //        Status = PaymentStatus.Pending.ToString(),
        //        TransactionType = TransactionType.Authorization.ToString(),
        //    };

        //    payment.SetMetaField(ResursConstants.ResursBankPaymentType, ResursPaymentMethod, false);
        //    payment.SetMetaField(ResursConstants.CardNumber, CardNumber, false);
        //    payment.SetMetaField(ResursConstants.GovernmentId, GovernmentId, false);
        //    payment.SetMetaField(ResursConstants.AmountForNewCard, AmountForNewCard, false);
        //    payment.SetMetaField(ResursConstants.InvoiceDeliveryType, InvoiceDeliveryType, false);

        //    return payment;
        //}

        public bool PostProcess(IOrderForm orderForm)
        {
            var resursPayment =
                orderForm.Payments.FirstOrDefault(x => x.PaymentMethodName.Equals("ResursBankCheckout"));

            // Check if payment frozen, if so set payment pending
            if (resursPayment == null)
            {
                return false;
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
                    addressRow2 = billingAddress.Line2,
                    postalArea = billingAddress.PostalCode,
                    postalCode = billingAddress.City
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
