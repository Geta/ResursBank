using System;
using System.Configuration;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Geta.Epi.Commerce.Payments.Resurs.Checkout.Extensions;
using Geta.Resurs.Checkout;
using Geta.Resurs.Checkout.ConfigurationService;
using Geta.Resurs.Checkout.Model;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Search;
using Newtonsoft.Json;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Callbacks
{
    public class ResursBankCallbackClient : IResursBankCallbackClient
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(ResursBankCallbackClient));

        private Injected<IResursHashCalculator> InjectedHashCalculator { get; set; }
        private Injected<IOrderRepository> InjectedOrderRepository { get; set; }

        private readonly ConfigurationWebServiceClient _configurationService;

        public ResursBankCallbackClient(ResursCredential credential)
        {
            _configurationService = new ConfigurationWebServiceClient();
            if (_configurationService.ClientCredentials != null)
            {
                if (credential != null)
                {
                    _configurationService.ClientCredentials.UserName.UserName = credential.UserName;
                    _configurationService.ClientCredentials.UserName.Password = credential.Password;
                }
                else
                {
                    var appSettings = ConfigurationManager.AppSettings;
                    _configurationService.ClientCredentials.UserName.UserName = appSettings["ResursBank:UserName"] ?? "Not Found";
                    _configurationService.ClientCredentials.UserName.Password = appSettings["ResursBank:Password"] ?? "Not Found";
                }
            }
        }

        public void RegisterCallbackUrl(CallbackEventType callbackEventType, string url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            var urlWithEventType = GetUrlWithEventType(callbackEventType, url);

            var currentCallback =
                _configurationService.getRegisteredEventCallback(callbackEventType.ToString());

            if (!string.IsNullOrWhiteSpace(currentCallback) && !currentCallback.Equals(urlWithEventType, StringComparison.InvariantCultureIgnoreCase))
            {
                _configurationService.unregisterEventCallback(callbackEventType.ToString());
            }

            _configurationService.registerEventCallback(
                callbackEventType.ToString(),
                urlWithEventType,
                null,
                null,
                new digestConfiguration
                {
                    digestAlgorithm = GetDigestAlgorithm(),
                    digestParameters = GetDigestParameters(),
                    digestSalt = GetSalt()
                });
        }

        public void UnRegisterCallbackUrl(CallbackEventType callbackEventType)
        {
            var currentCallback =
                _configurationService.getRegisteredEventCallback(callbackEventType.ToString());

            if (!string.IsNullOrWhiteSpace(currentCallback))
            {
                _configurationService.unregisterEventCallback(callbackEventType.ToString());
            }
        }

        public void ProcessCallback(CallbackData callbackData, string digest)
        {
            if (callbackData == null) throw new ArgumentNullException(nameof(callbackData));
            if (digest == null) throw new ArgumentNullException(nameof(digest));

            _logger.Information($"ProcessCallback: Start processing {JsonConvert.SerializeObject(callbackData)} {digest}");

            if (!CheckDigest(callbackData, digest))
            {
                _logger.Debug($"ProcessCallback: Wrong digest {digest} for {JsonConvert.SerializeObject(callbackData)}");
                throw new ArgumentException(nameof(digest));
            }

            var order = GetOrderByPayment(callbackData.PaymentId);
            if (order == null)
            {
                _logger.Debug($"ProcessCallback: Can't find order for {JsonConvert.SerializeObject(callbackData)}");
                throw new ArgumentException(nameof(callbackData.PaymentId));
            }
            var payment = GetPayment(callbackData, order);
            if (payment == null)
            {
                _logger.Debug($"ProcessCallback: Can't find payment for {JsonConvert.SerializeObject(callbackData)}");
                throw new ArgumentException(nameof(callbackData.PaymentId));
            }

            bool processed;
            switch (callbackData.EventType)
            {
                case CallbackEventType.UNFREEZE:
                    processed = ProcessUnfreeze(order, payment);
                    break;
                case CallbackEventType.ANNULMENT:
                    processed = ProcessAnnulment(order, payment);
                    break;
                case CallbackEventType.BOOKED:
                case CallbackEventType.AUTOMATIC_FRAUD_CONTROL:
                case CallbackEventType.FINALIZATION:
                case CallbackEventType.UPDATE:
                case CallbackEventType.TEST:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!processed)
            {
                _logger.Information($"ProcessCallback: Didn't process {JsonConvert.SerializeObject(callbackData)}");
                return;
            }

            // Add order note
            var message = $"ResursBankCallback: processed {callbackData.EventType}";
            var note = order.CreateOrderNote();
            note.Title = message;
            note.Detail = message;

            order.Notes.Add(note);

            _logger.Information($"ProcessCallback: Processed {JsonConvert.SerializeObject(callbackData)}");
        }

        protected virtual bool CheckOrderFrozenStatus(IPurchaseOrder order, IPayment payment)
        {
            if (order.OrderStatus.Equals(OrderStatus.OnHold) && payment.GetResursFreezeStatus())
            {
                return true;
            }

            _logger.Information($"Order not on hold or payment not frozen. ResursPaymentId: {payment.Properties[ResursConstants.ResursPaymentId]} PO: {order.OrderNumber} OrderGroupId: {order.OrderLink.OrderGroupId}");

            return false;
        }

        protected virtual bool ProcessUnfreeze(IPurchaseOrder order, IPayment payment)
        {
            if (!CheckOrderFrozenStatus(order, payment)) return false;

            order.OrderStatus = OrderStatus.InProgress;
            payment.Properties[ResursConstants.PaymentFreezeStatus] = false;
            payment.Status = PaymentStatus.Processed.ToString();

            order.AddNote($"Resurs: UNFREEZE {payment.GetResursPaymentId()}");

            InjectedOrderRepository.Service.Save(order);

            return true;
        }

        protected virtual bool ProcessAnnulment(IPurchaseOrder order, IPayment payment)
        {
            if (!CheckOrderFrozenStatus(order, payment)) return false;

            order.OrderStatus = OrderStatus.Cancelled;
            payment.Properties[ResursConstants.PaymentFreezeStatus] = false;
            payment.Status = PaymentStatus.Failed.ToString();

            order.AddNote($"Resurs: ANNULMENT {payment.GetResursPaymentId()}");

            InjectedOrderRepository.Service.Save(order);

            return true;
        }

        private IPayment GetPayment(CallbackData callbackData, IPurchaseOrder order)
        {
            return order.Forms.SelectMany(x => x.Payments)
                              .FirstOrDefault(payment => callbackData.PaymentId.Equals(payment.Properties[ResursConstants.ResursPaymentId]));
        }

        protected virtual IPurchaseOrder GetOrderByPayment(string paymentId)
        {
            var searchOptions = new OrderSearchOptions
            {
                CacheResults = false,
                StartingRecord = 0,
                RecordsToRetrieve = 1
            };
            searchOptions.Classes.Add("PurchaseOrder");

            var parameters = new OrderSearchParameters
            {
                SqlWhereClause = "OrderGroupId IN " +
                                 "(SELECT OrderGroupId FROM [OrderFormPayment] WHERE [PaymentId] IN " +
                                 "(SELECT ObjectId FROM [OrderFormPayment_Other] WHERE [ResursPaymentId] = " +
                                $"'{paymentId}'" +
                                 "))"
            };

            var purchaseOrderCollection = OrderContext.Current.FindPurchaseOrders(parameters, searchOptions);
            return purchaseOrderCollection.FirstOrDefault();
        }

        protected virtual string GetUrlWithEventType(CallbackEventType callbackEventType, string url)
        {
            return url.Replace("{eventType}", callbackEventType.ToString());
        }

        public virtual bool CheckDigest(CallbackData callbackData, string digest)
        {
            if (string.IsNullOrWhiteSpace(digest))
            {
                return false;
            }
            return digest.Equals(InjectedHashCalculator.Service.Compute(callbackData, GetSalt()));
        }

        public void ProcessFrozenPayments(IPurchaseOrder purchaseOrder)
        {
            // Hold order and set payment(s) to pending
            var frozenPayments =
                purchaseOrder
                    .Forms
                    .SelectMany(x => x.Payments)
                    .Where(payment => payment.GetResursFreezeStatus())
                    .ToList();

            if (!frozenPayments.Any())
            {
                _logger.Information($"ProcessFrozenPayments: no frozen payments for order {purchaseOrder.OrderNumber} - {purchaseOrder.OrderLink.OrderGroupId}");
                return;
            }

            foreach (var frozenPayment in frozenPayments)
            {
                frozenPayment.Status = PaymentStatus.Pending.ToString();
                purchaseOrder.AddNote($"Resurs: Order on hold due to FROZEN payment status {frozenPayment.GetResursPaymentId()}");
            }

            purchaseOrder.OrderStatus = OrderStatus.OnHold;
            
            InjectedOrderRepository.Service.Save(purchaseOrder);

            _logger.Information($"ProcessFrozenPayments: {purchaseOrder.OrderNumber} - {purchaseOrder.OrderLink.OrderGroupId}");
        }

        protected virtual digestAlgorithm GetDigestAlgorithm()
        {
            return digestAlgorithm.SHA1;
        }

        protected virtual string GetSalt()
        {
            return ConfigurationManager.AppSettings["ResursBank:CallbackDigestSalt"];
        }

        protected virtual string[] GetDigestParameters()
        {
            return new[] { "paymentId" };
        }
    }
}