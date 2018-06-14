namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Business
{
    public interface IResursBankPaymentInfo
    {
        decimal? AmountForNewCard { get; set; }
        string GovernmentId { get; set; }
        string InvoiceDeliveryType { get; set; }
        string ResursPaymentMethod { get; set; }
        string SigningSuccessUrl { get; set; }
        string SigningFailedUrl { get; set; }
    }
}