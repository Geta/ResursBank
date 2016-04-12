using System;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using Geta.Resurs.Checkout;
using Mediachase.Commerce.Catalog;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;

namespace Geta.EPi.Commerce.Payments.Resurs.Checkout
{
    /// <summary>
    /// TODO clean this up and don't repeat code
    /// </summary>
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

            var creditAmountForNewCard = GetOrCreateAmountForNewCardField(mdContext);
            JoinField(mdContext, creditAmountForNewCard, ResursConstants.OtherPaymentClass);

            var minLimit = GetOrCreateMinLimitField(mdContext);
            JoinField(mdContext, minLimit, ResursConstants.OtherPaymentClass);

            var maxLimit = GetOrCreateMaxLimitField(mdContext);
            JoinField(mdContext, maxLimit, ResursConstants.OtherPaymentClass);

            var callbackUrl = GetOrCreateCallBackField(mdContext);
            JoinField(mdContext, callbackUrl, ResursConstants.OtherPaymentClass);

            var resursVatPercent = GetOrCreateResursVatPercentField(mdContext);
            JoinField(mdContext, resursVatPercent, ResursConstants.LineItemExClass);

            var deliveryType = GetOrCreateInvoiceDeliveryTypeField(mdContext);
            JoinField(mdContext, deliveryType, ResursConstants.OtherPaymentClass);
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

            var f = MetaField.Load(mdContext, ResursConstants.GovernmentId);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.GovernmentId));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.GovernmentId, ResursConstants.GovernmentId, string.Empty, MetaDataType.ShortString, 255, true, false, false, false);
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

        private MetaField GetOrCreateAmountForNewCardField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, ResursConstants.AmountForNewCard);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.AmountForNewCard));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.AmountForNewCard, ResursConstants.AmountForNewCard, string.Empty, MetaDataType.ShortString, Int32.MaxValue, true, false, false, false);
            }

            return f;
        }

        private MetaField GetOrCreateMinLimitField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, ResursConstants.MinLimit);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.MinLimit));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.MinLimit, ResursConstants.MinLimit, string.Empty, MetaDataType.ShortString, Int32.MaxValue, true, false, false, false);
            }

            return f;
        }

        private MetaField GetOrCreateMaxLimitField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, ResursConstants.MaxLimit);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.MaxLimit));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.MaxLimit, ResursConstants.MaxLimit, string.Empty, MetaDataType.ShortString, Int32.MaxValue, true, false, false, false);
            }

            return f;
        }

        private MetaField GetOrCreateCallBackField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, ResursConstants.CallBackUrl);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.CallBackUrl));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.CallBackUrl, ResursConstants.CallBackUrl, string.Empty, MetaDataType.LongString, Int32.MaxValue, true, false, false, false);
            }

            return f;
        }

        private MetaField GetOrCreateResursVatPercentField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, ResursConstants.ResursVatPercent);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.ResursVatPercent));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.ResursVatPercent, ResursConstants.ResursVatPercent, string.Empty, MetaDataType.Decimal, 0, true, false, false, false);
            }

            return f;
        }

        private MetaField GetOrCreateInvoiceDeliveryTypeField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, ResursConstants.InvoiceDeliveryType);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", ResursConstants.InvoiceDeliveryType));
                f = MetaField.Create(mdContext, ResursConstants.OrderNamespace, ResursConstants.InvoiceDeliveryType, ResursConstants.InvoiceDeliveryType, string.Empty, MetaDataType.ShortString, Int32.MaxValue, true, false, false, false);
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
    }
}
