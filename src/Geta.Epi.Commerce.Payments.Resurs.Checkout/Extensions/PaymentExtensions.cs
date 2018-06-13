using System;
using EPiServer.Commerce.Order;
using Geta.Resurs.Checkout;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Extensions
{
    public static class PaymentExtensions
    {
        public static bool GetResursFreezeStatus(this IPayment resursPayment, bool fallback = false)
        {
            try
            {
                return resursPayment.Properties[ResursConstants.PaymentFreezeStatus] as bool? ?? false;
            }
            catch (Exception)
            {
                return fallback;
            }
        }
    }
}