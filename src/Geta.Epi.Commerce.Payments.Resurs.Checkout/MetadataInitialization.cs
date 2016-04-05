using System;
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
            JoinField(mdContext, resursBankPaymentMethod, ResursConstants.ResursBankPayment);

            var gorvernmentId = GetOrCreateGorvernmentIdField(mdContext);
            JoinField(mdContext, gorvernmentId, ResursConstants.OtherPaymentClass);

            var orderId = GetOrCreateOrderIdField(mdContext);
            JoinField(mdContext, orderId, ResursConstants.OtherPaymentClass);

            var resursBankPaymentType = GetOrCreateResursBankPaymentTypeField(mdContext);
            JoinField(mdContext, resursBankPaymentType, ResursConstants.OtherPaymentClass);

            var cardNumber = GetOrCreateCardNumberField(mdContext);
            JoinField(mdContext, cardNumber, ResursConstants.OtherPaymentClass);

            var customerIpAddress = GetOrCreateCustomerIpAddressField(mdContext);
            JoinField(mdContext, customerIpAddress, ResursConstants.OtherPaymentClass);

            var successUrl = GetOrCreateSuccessUrlField(mdContext);
            JoinField(mdContext, successUrl, ResursConstants.OtherPaymentClass);

            var failUrl = GetOrCreateFailUrlField(mdContext);
            JoinField(mdContext, failUrl, ResursConstants.OtherPaymentClass);
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

            var f = MetaField.Load(mdContext, ResursConstants.GorvernmentId);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.GorvernmentId));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.GorvernmentId, ResursConstants.GorvernmentId, string.Empty, MetaDataType.ShortString, 255, true, false, false, false);
            }

            return f;
        }

        private MetaField GetOrCreateOrderIdField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, ResursConstants.OrderId);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.OrderId));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.OrderId, ResursConstants.OrderId, string.Empty, MetaDataType.ShortString, 255, true, false, false, false);
            }

            return f;
        }

        private MetaField GetOrCreateResursBankPaymentTypeField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, ResursConstants.ResursBankPaymentType);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.ResursBankPaymentType));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.ResursBankPaymentType, ResursConstants.ResursBankPaymentType, string.Empty, MetaDataType.ShortString, 255, true, false, false, false);
            }

            return f;
        }

        private MetaField GetOrCreateCardNumberField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, ResursConstants.CardNumber);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.CardNumber));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.CardNumber, ResursConstants.CardNumber, string.Empty, MetaDataType.ShortString, 255, true, false, false, false);
            }

            return f;
        }

        private MetaField GetOrCreateCustomerIpAddressField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, ResursConstants.CustomerIpAddress);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.CustomerIpAddress));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.CustomerIpAddress, ResursConstants.CustomerIpAddress, string.Empty, MetaDataType.ShortString, 255, true, false, false, false);
            }

            return f;
        }

        private MetaField GetOrCreateSuccessUrlField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, ResursConstants.SuccessfullUrl);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.SuccessfullUrl));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.SuccessfullUrl, ResursConstants.SuccessfullUrl, string.Empty, MetaDataType.LongString, Int32.MaxValue, true, false, false, false);
            }

            return f;
        }

        private MetaField GetOrCreateFailUrlField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, ResursConstants.FailBackUrl);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.FailBackUrl));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.FailBackUrl, ResursConstants.FailBackUrl, string.Empty, MetaDataType.LongString, Int32.MaxValue, true, false, false, false);
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
