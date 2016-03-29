using System.Web.Mvc;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Geta.Epi.Commerce.Payments.Resurs.Checkout.Initialization;
using Geta.Resurs.Checkout;
using StructureMap;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Bussiness.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class DependencyResolverInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Container.Configure(ConfigureContainer);

            DependencyResolver.SetResolver(new StructureMapDependencyResolver(context.Container));
        }

        private static void ConfigureContainer(ConfigurationExpression container)
        {
            //Implementations for custom interfaces can be registered here.
            container.For<IResursBankServiceSettingFactory>().Use<ResursBankServiceSettingFactory>().Singleton();
            container.For<IResursBankServiceClient>().Use<ResursBankServiceClient>().Singleton();
        }

        public void Initialize(InitializationEngine context)
        {
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }
    }
}
