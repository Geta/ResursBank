using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geta.Resurs.Checkout.Model;
using Geta.Resurs.Checkout.SimplifiedShopFlowService;

namespace Geta.Resurs.Checkout
{
    public interface IResursBankServiceClient
    {
        List<PaymentMethodResponse> GetPaymentMethods(string lang, string custType, decimal amount);

        bookPaymentResult BookPayment(string paymentMethodId, string customerIpAddress, List<SpecLine> specLines,
           Customer customer, Card card, signing _signing, string callBackUrl);
        bookPaymentResult BookSignedPayment(string paymentId);
        address GetAddress(string governmentId, string customerType, string customerIpAddress);
    }
}
