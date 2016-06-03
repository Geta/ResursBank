using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using EPiServer.ServiceLocation;
using Geta.Resurs.Checkout.Model;
using Geta.Resurs.Checkout.SimplifiedShopFlowService;
using address = Geta.Resurs.Checkout.SimplifiedShopFlowService.address;
using customerType = Geta.Resurs.Checkout.SimplifiedShopFlowService.customerType;


namespace Geta.Resurs.Checkout
{
    [ServiceConfiguration(typeof(IResursBankServiceClient), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ResursBankServiceClient : IResursBankServiceClient
    {
        private readonly SimplifiedShopFlowWebServiceClient _shopServiceClient;

        public ResursBankServiceClient(ResursCredential credential)
        {
            _shopServiceClient = new SimplifiedShopFlowWebServiceClient();
            if (_shopServiceClient.ClientCredentials != null)
            {
                if (credential != null)
                {
                    _shopServiceClient.ClientCredentials.UserName.UserName = credential.UserName;
                    _shopServiceClient.ClientCredentials.UserName.Password = credential.Password;
                }
                else
                {
                    var appSettings = ConfigurationManager.AppSettings;
                    _shopServiceClient.ClientCredentials.UserName.UserName = appSettings["ResursBank:UserName"] ?? "Not Found";
                    _shopServiceClient.ClientCredentials.UserName.Password = appSettings["ResursBank:Password"] ?? "Not Found";
                }
            }
            
        }

        public List<PaymentMethodResponse> GetPaymentMethods(string lang, string custType, decimal amount)
        {
            if (_shopServiceClient == null || _shopServiceClient.ClientCredentials == null)
            {
                return null;
            }

            var paymentMethodList = new List<PaymentMethodResponse>();
            language langEnum = (language)Enum.Parse(typeof(language), lang);
            customerType customerTypeEnum = (customerType)Enum.Parse(typeof(customerType), custType);

            var paymentMethods = _shopServiceClient.getPaymentMethods(langEnum, customerTypeEnum, amount);
            _shopServiceClient.Close();
            foreach (paymentMethod paymentMethod in paymentMethods)
            {
                WebLink[] legealInfoLinks = null;

                if (paymentMethod.legalInfoLinks != null)
                {
                    legealInfoLinks = paymentMethod.legalInfoLinks.Select(l => new WebLink(l.appendPriceLast, l.endUserDescription, l.url)).ToArray();
                }

                var paymentMethodResponse = new PaymentMethodResponse(paymentMethod.id, paymentMethod.description, paymentMethod.minLimit, 
                    paymentMethod.maxLimit, paymentMethod.specificType, legealInfoLinks);
                paymentMethodList.Add(paymentMethodResponse);
            }
            return paymentMethodList;

        }
        
        public bookPaymentResult BookPayment(BookPaymentObject bookPaymentObject)
        {
            try
            {
               return _shopServiceClient.bookPayment(bookPaymentObject.PaymentData, bookPaymentObject.PaymentSpec, bookPaymentObject.MapEntry, bookPaymentObject.ExtendedCustomer,bookPaymentObject.Card, bookPaymentObject.Signing,bookPaymentObject.InvoiceData, bookPaymentObject.CallbackUrl);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public bookPaymentResult BookSignedPayment(string paymentId)
        {
            return _shopServiceClient.bookSignedPayment(paymentId);
        }
        
        public address GetAddress(string governmentId, string customerType, string customerIpAddress)
        {
            customerType cType = (customerType)Enum.Parse(typeof(customerType), customerType);
            return _shopServiceClient.getAddress(governmentId, cType, customerIpAddress);
        }
    }
}
