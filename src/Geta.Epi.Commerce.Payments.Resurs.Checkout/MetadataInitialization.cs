using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using Geta.Epi.Commerce.Payments.Resurs.Checkout;
using Mediachase.Commerce.Catalog;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;

namespace Geta.EPi.Commerce.Payments.Resurs.Checkout
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    public class MetadataInitialization : IInitializableModule
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(MetadataInitialization));

        public void Initialize(InitializationEngine context)
        {
            MetaDataContext mdContext = CatalogContext.MetaDataContext;

            var resursBankPaymentMethod = GetOrCreateResursBankPaymentMethodField(mdContext);
            JoinField(mdContext, resursBankPaymentMethod, ResursConstants.OtherPaymentClass);

            var gorvernmentId = GetOrCreateGorvernmentIdField(mdContext);
            JoinField(mdContext, gorvernmentId, ResursConstants.OtherPaymentClass);
        }


        private MetaField GetOrCreateResursBankPaymentMethodField(MetaDataContext mdContext)
        {
            
            var f = MetaField.Load(mdContext, ResursConstants.ResursBankPaymentMethod);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.ResursBankPaymentMethod));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.ResursBankPaymentMethod, ResursConstants.ResursBankPaymentMethod, string.Empty, MetaDataType.ShortString, 255, true, false, false, false);
            }

            return f;
        }

        private MetaField GetOrCreateGorvernmentIdField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, ResursConstants.ResursBankPaymentMethod);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.GorvernmentId));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.GorvernmentId, ResursConstants.ResursBankPaymentMethod, string.Empty, MetaDataType.ShortString, 255, true, false, false, false);
            }

            return f;
        }

        private void JoinField(MetaDataContext mdContext, MetaField field, string metaClassName)
        {
            var cls = MetaClass.Load(mdContext, metaClassName);

            if (MetaFieldIsNotConnected(field, cls))
            {
                cls.AddField(field);
            }
        }

        private static bool MetaFieldIsNotConnected(MetaField field, MetaClass cls)
        {
            return cls != null && !cls.MetaFields.Contains(field);
        }

        public void Uninitialize(InitializationEngine context)
        {
            
        }

        public void Preload(string[] parameters)
        {
            
        }
    }
}
