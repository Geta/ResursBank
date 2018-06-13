namespace Geta.Epi.Commerce.Payments.Resurs.Checkout
{
    public interface IResursBankRedirectSettings
    {
        string SuccessRedirectUrl { get; }
        string FailureCallbackUrl { get; }
        string CallbackUrl { get; }
    }
}