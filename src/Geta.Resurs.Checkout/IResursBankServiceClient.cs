using System.Collections.Generic;
using Geta.Resurs.Checkout.Model;
using Geta.Resurs.Checkout.SimplifiedShopFlowService;

namespace Geta.Resurs.Checkout
{
    public interface IResursBankServiceClient
    {
        List<PaymentMethodResponse> GetPaymentMethods(string lang, string custType, decimal amount);
        bookPaymentResult BookPayment(BookPaymentObject bookPaymentObject);
        bookPaymentResult BookSignedPayment(string paymentId);
        address GetAddress(string governmentId, string customerType, string customerIpAddress);
    }
}
