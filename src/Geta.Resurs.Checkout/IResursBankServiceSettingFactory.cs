using Geta.Resurs.Checkout.Model;

namespace Geta.Resurs.Checkout
{
    public interface IResursBankServiceSettingFactory
    {
        ResursBankServiceClient Init(ResursCredential credential);
    }
}
