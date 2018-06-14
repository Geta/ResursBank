namespace Geta.Epi.Commerce.Payments.Resurs.Checkout
{
    public interface IResursBankRedirectSettings
    {
        string SigningSuccessUrl { get; }
        string SigningFailedUrl { get; }
    }
}