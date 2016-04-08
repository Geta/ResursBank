using Geta.Resurs.Checkout.Model;

namespace Geta.Resurs.Checkout
{
    public class ResursBankServiceSettingFactory: IResursBankServiceSettingFactory
    {
        public ResursBankServiceClient Init(ResursCredential credential)
        {
            return new ResursBankServiceClient(credential);
        }
    }
}
