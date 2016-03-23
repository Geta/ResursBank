using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geta.Resurs.Checkout.Model;
using Geta.Resurs.Checkout.SimplifiedShopFlowService;

namespace Geta.Resurs.Checkout
{
    public class ResursBankServiceClient
    {
        public List<PaymentMethod> GetPaymentMethods(string lang, string custType, decimal amount)
        {
            var paymentMethodList = new List<PaymentMethod>();
            language langEnum = (language)System.Enum.Parse(typeof(language), lang);
            customerType customerTypeEnum = (customerType)System.Enum.Parse(typeof(customerType), custType);
            using (var client = new SimplifiedShopFlowWebServiceClient())
            {
                if (client.ClientCredentials != null)
                {
                    var appSettings = ConfigurationManager.AppSettings;
                    string username = appSettings["username"] ?? "Not Found";
                    string password = appSettings["password"] ?? "Not Found";

                    client.ClientCredentials.UserName.UserName = username;
                    client.ClientCredentials.UserName.Password = password;
                }
                var paymentMethods = client.getPaymentMethods(langEnum, customerTypeEnum, 1000);
                foreach (var paymentMethod in paymentMethods)
                {
                    //paymentMethodList.Add(new PaymentMethod() { });
                    var pMethod = new PaymentMethod();
                    pMethod.Id = paymentMethod.id;
                    //pMethod.CustomerTypeField = paymentMethod.customerType;
                    pMethod.DescriptionField = paymentMethod.description;
                    pMethod.MaxLimitField = paymentMethod.maxLimit;

                    paymentMethodList.Add(pMethod);
                }
                return paymentMethodList;
            }


        }
    }
}
