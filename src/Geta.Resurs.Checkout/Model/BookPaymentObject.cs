using Geta.Resurs.Checkout.SimplifiedShopFlowService;

namespace Geta.Resurs.Checkout.Model
{
    public class BookPaymentObject
    {
        public paymentData PaymentData { get; set; }
        public paymentSpec PaymentSpec { get; set; } 
        public mapEntry[] MapEntry { get; set; }
        public extendedCustomer ExtendedCustomer { get; set; }
        public cardData Card { get; set; }
        public signing Signing { get; set; }
        public invoiceData InvoiceData { get; set; }
        public string CallbackUrl { get; set; }
    }
}