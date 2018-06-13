namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Business
{
    public class ResursCheckoutPaymentErrors
    {
        public const string Configuration = "Wrong configuration, configure resursbank in appsettings.";
        public const string PaymentType = "Payment is not of type ResursBankPayment";
        public const string PaymentDenied = "Booking of payment was denied.";
        public const string UnknownPaymentStatus = "Unknown bookPaymentStatus.";
        public const string Generic = "Something went wrong trying to process the payment.";
    }
}
