using System;
using System.Collections.Generic;
using System.Configuration;
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
        private SimplifiedShopFlowWebServiceClient _shopServiceClient;



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
                    _shopServiceClient.ClientCredentials.UserName.UserName = appSettings["ResursBankUserName"] ?? "Not Found";
                    _shopServiceClient.ClientCredentials.UserName.Password = appSettings["ResursBankUserNamePassword"] ?? "Not Found";
                }
            }
            
        }

        public List<PaymentMethodResponse> GetPaymentMethods(string lang, string custType, decimal amount)
        {
            if (_shopServiceClient == null || _shopServiceClient.ClientCredentials == null)
                return null;

            var paymentMethodList = new List<PaymentMethodResponse>();
            language langEnum = (language)System.Enum.Parse(typeof(language), lang);
            customerType customerTypeEnum = (customerType)System.Enum.Parse(typeof(customerType), custType);

            var paymentMethods = _shopServiceClient.getPaymentMethods(langEnum, customerTypeEnum, amount);
            _shopServiceClient.Close();
            foreach (var paymentMethod in paymentMethods)
            {
                var paymentMethodResponse = new PaymentMethodResponse(paymentMethod.id, paymentMethod.description, paymentMethod.minLimit, paymentMethod.maxLimit, paymentMethod.specificType);
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
            customerType cType = (customerType)System.Enum.Parse(typeof(customerType), customerType);
            return _shopServiceClient.getAddress(governmentId, cType, customerIpAddress);
        }
    }
}
