using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using EPiServer.Framework.Cache;
using EPiServer.ServiceLocation;
using Geta.Resurs.Checkout;
using Geta.Resurs.Checkout.Model;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Business
{
    [ServiceConfiguration(typeof(IResursBankPaymentMethodService))]
    public class ResursBankPaymentMethodService : IResursBankPaymentMethodService
    {
        internal Injected<ISynchronizedObjectInstanceCache> InjectedCache { get; set; }
        private ISynchronizedObjectInstanceCache _cache => InjectedCache.Service;
        internal Injected<IResursBankServiceSettingFactory> InjectedResursBankServiceSettingFactory { get; set; }
        private IResursBankServiceSettingFactory _resursBankServiceSettingFactory =>
            InjectedResursBankServiceSettingFactory.Service;

        public ResursBankPaymentMethodService()
        {
        }

        public List<PaymentMethodResponse> GetResursPaymentMethods(string lang, string custType, decimal amount)
        {
            var cacheKey = $"GetListResursPaymentMethods:{lang}:{custType}";
            var methods = _cache.Get(cacheKey) as List<PaymentMethodResponse>;
            if (methods == null || !methods.Any())
            {
                var credential = new ResursCredential(ConfigurationManager.AppSettings["ResursBank:UserName"],
                    ConfigurationManager.AppSettings["ResursBank:Password"]);
                var resursBankServices = _resursBankServiceSettingFactory.Init(credential);
                methods = resursBankServices.GetPaymentMethods(lang, custType, null);
                //Cache list payment methods for 1 day as Resurs recommended.
                _cache.Insert(cacheKey, methods, new CacheEvictionPolicy(new TimeSpan(1, 0, 0, 0), CacheTimeoutType.Absolute));
            }

            return
                methods
                    .Where(x =>
                        x.MinLimitField <= amount &&
                        x.MaxLimitField >= amount)
                    .ToList();
        }
    }
}