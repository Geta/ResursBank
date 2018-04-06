using System.Configuration;
using EPiServer.ServiceLocation;
using Geta.Resurs.Checkout.Model;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Callbacks
{
    [ServiceConfiguration(typeof(IResursBankCallbackClientFactory), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ResursBankCallbackClientFactory : IResursBankCallbackClientFactory
    {
        public IResursBankCallbackClient Init(ResursCredential credential)
        {
            return new ResursBankCallbackClient(credential);
        }

        public IResursBankCallbackClient Init()
        {
            return new ResursBankCallbackClient(new ResursCredential(
                ConfigurationManager.AppSettings["ResursBank:UserName"],
                ConfigurationManager.AppSettings["ResursBank:Password"]));
        }
    }
}