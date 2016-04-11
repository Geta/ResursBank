using EPiServer.ServiceLocation;
using Geta.Resurs.Checkout.Model;

namespace Geta.Resurs.Checkout
{
    [ServiceConfiguration(typeof(IResursBankServiceSettingFactory), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ResursBankServiceSettingFactory: IResursBankServiceSettingFactory
    {
        public ResursBankServiceClient Init(ResursCredential credential)
        {
            return new ResursBankServiceClient(credential);
        }
    }
}
