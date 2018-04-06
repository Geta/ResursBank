namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Callbacks
{
    public interface IResursHashCalculator
    {
        string Compute(CallbackData parameters, string salt);
    }
}