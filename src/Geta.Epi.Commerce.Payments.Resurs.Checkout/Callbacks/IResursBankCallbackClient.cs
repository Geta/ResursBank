using EPiServer.Commerce.Order;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Callbacks
{
    public interface IResursBankCallbackClient
    {
        void RegisterCallbackUrl(CallbackEventType callbackEventType, string url);
        void UnRegisterCallbackUrl(CallbackEventType callbackEventType);
        void ProcessCallback(CallbackData callbackData, string digest);
        bool CheckDigest(CallbackData callbackData, string digest);
        void ProcessFrozenPayments(IPurchaseOrder purchaseOrder);
    }
}