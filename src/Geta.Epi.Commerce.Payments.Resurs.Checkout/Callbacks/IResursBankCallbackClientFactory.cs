using Geta.Resurs.Checkout.Model;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Callbacks
{
    public interface IResursBankCallbackClientFactory
    {
        IResursBankCallbackClient Init();
        IResursBankCallbackClient Init(ResursCredential resursCredential);
    }
}