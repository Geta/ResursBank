using System;
using System.Linq;
using System.Runtime.Serialization;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Geta.Resurs.Checkout;
using Geta.Resurs.Checkout.Model;
using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Business
{
    [Serializable]
    public class ResursBankPayment : OtherPayment
    {
        private Injected<IOrderGroupFactory> InjectedOrderGroupFactory { get; set; }
        private Injected<IResursBankPaymentMethodService> InjectedResursBankPaymentMethodService { get; set; }

        public ResursBankPayment() { }

        public ResursBankPayment(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IPayment CreatePayment(
            decimal amount, 
            IOrderGroup orderGroup,
            PaymentMethodResponse paymentMethod,
            ResursBankPaymentInfo resursBankPaymentInfo,
            out string validationMessage,
            string language = "no",
            string customerType = "NATURAL")
        {
            if (Validate(amount, resursBankPaymentInfo, out validationMessage, language, customerType)) return null;

            var payment = orderGroup.CreatePayment(InjectedOrderGroupFactory.Service, typeof(ResursBankPayment));
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = ResursConstants.SystemName;
            payment.Amount = amount;
            payment.PaymentType = PaymentType.Other;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = Mediachase.Commerce.Orders.TransactionType.Authorization.ToString();

            payment.SetPaymentData(resursBankPaymentInfo);

            validationMessage = null;
            return payment;
        }

        private bool Validate(
            decimal amount,
            ResursBankPaymentInfo resursBankPaymentInfo,
            out string validationMessage,
            string language = "no",
            string customerType = "NATURAL")
        {
            var method = InjectedResursBankPaymentMethodService.Service
                .GetResursPaymentMethods(language, customerType, amount)
                .FirstOrDefault(x => x.Id.Equals(resursBankPaymentInfo.ResursPaymentMethod));
            if (method == null)
            {
                validationMessage = $"Unknown payment method";
                return false;
            }

            //validate
            if (amount > method.MaxLimitField || amount < method.MinLimitField)
            {
                //not valid
                validationMessage = $"total is not in limit from {method.MinLimitField} to {method.MaxLimitField}";
                return false;
            }

            if (string.IsNullOrWhiteSpace(resursBankPaymentInfo.SigningSuccessUrl) ||
                string.IsNullOrWhiteSpace(resursBankPaymentInfo.SigningFailedUrl))
            {
                validationMessage = $"Please configure IResursbankRedirectSettings";
                return false;
            }

            validationMessage = null;
            return true;
        }
    }

    public static class Temp
    {
        public static void SetPaymentData(this IPayment payment, ResursBankPaymentInfo resursBankPaymentInfo)
        {
                payment.Properties[ResursConstants.ResursBankPaymentType] = resursBankPaymentInfo.ResursPaymentMethod;
                payment.Properties[ResursConstants.GovernmentId] = resursBankPaymentInfo.GovernmentId;
                payment.Properties[ResursConstants.AmountForNewCard] = resursBankPaymentInfo.AmountForNewCard;
                payment.Properties[ResursConstants.InvoiceDeliveryType] = resursBankPaymentInfo.InvoiceDeliveryType;
        }
    }

    public class ResursBankPaymentInfo : IResursBankPaymentInfo
    {
        public string ResursPaymentMethod { get; set; }
        public string GovernmentId { get; set; }
        public decimal? AmountForNewCard { get; set; }
        public string SigningSuccessUrl { get; set; }
        public string SigningFailedUrl { get; set; }
        public string InvoiceDeliveryType { get; set; }
    }
}
