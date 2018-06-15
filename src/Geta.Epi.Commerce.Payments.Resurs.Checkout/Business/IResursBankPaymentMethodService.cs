using System.Collections.Generic;
using Geta.Resurs.Checkout.Model;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Business
{
    public interface IResursBankPaymentMethodService
    {
        List<PaymentMethodResponse> GetResursPaymentMethods(string lang, string custType, decimal amount);
    }
}