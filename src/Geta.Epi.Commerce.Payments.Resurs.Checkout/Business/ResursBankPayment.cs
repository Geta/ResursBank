using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Geta.Resurs.Checkout;
using Geta.Resurs.Checkout.Model;
using Geta.Resurs.Checkout.SimplifiedShopFlowService;
using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Business
{
    [Serializable]
    public class ResursBankPayment : OtherPayment
    {
        private Injected<IOrderGroupFactory> InjectedOrderGroupFactory { get; set; }

        public ResursBankPayment() { }

        public ResursBankPayment(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IPayment CreatePayment(decimal amount, IOrderGroup orderGroup, PaymentMethodResponse paymentMethod, ResursBankPaymentInfo resursBankPaymentInfo, out string validationMessage)
        {
            if (Validate(amount, resursBankPaymentInfo, out validationMessage)) return null;

            var payment = orderGroup.CreatePayment(InjectedOrderGroupFactory.Service, typeof(ResursBankPayment));
            //payment.PaymentMethodId = PaymentMethodId;
            //payment.PaymentMethodName = "ResursBankCheckout";
            payment.Amount = amount;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = Mediachase.Commerce.Orders.TransactionType.Authorization.ToString();

            payment.SetPaymentData(resursBankPaymentInfo);

            validationMessage = null;
            return payment;
        }

        private static bool Validate(decimal amount, ResursBankPaymentInfo resursBankPaymentInfo, out string validationMessage)
        {
            //validate
            //if (amount > resursBankPaymentInfo.MaxLimit || amount < resursBankPaymentInfo.MinLimit)
            //{
            //    //not valid
            //    validationMessage =
            //        $"total is not in limit from {resursBankPaymentInfo.MinLimit} to {resursBankPaymentInfo.MaxLimit}";
            //    return true;
            //}

            //if (string.IsNullOrWhiteSpace(resursBankPaymentInfo.SigningSuccessUrl) ||
            //    string.IsNullOrWhiteSpace(resursBankPaymentInfo.SigningFailedUrl))
            //{
            //    validationMessage = $"Please configure IResursbankRedirectSettings";
            //    return true;
            //}

            validationMessage = null;
            return false;
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
