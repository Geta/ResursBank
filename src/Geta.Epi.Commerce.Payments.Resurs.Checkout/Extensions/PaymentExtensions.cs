using System;
using Geta.Resurs.Checkout;
using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Extensions
{
    public static class PaymentExtensions
    {
        public static bool GetResursFreezeStatus(this Payment resursPayment, bool fallback = false)
        {
            try
            {
                return resursPayment.GetBool(ResursConstants.PaymentFreezeStatus);
            }
            catch (Exception)
            {
                return fallback;
            }
        }
    }
}