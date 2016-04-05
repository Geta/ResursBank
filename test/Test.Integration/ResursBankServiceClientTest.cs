using System;
using System.Text;
using System.Collections.Generic;
using System.Configuration;
using EPiServer.ServiceLocation;
using Geta.Resurs.Checkout;
using Geta.Resurs.Checkout.AfterShopFlowService;
using Geta.Resurs.Checkout.Model;
using Xunit;


namespace Test.Integration
{
    /// <summary>
    /// Summary description for ResursBankServiceClientTest
    /// </summary>
   
    public class ResursBankServiceClientTest
    {
        [Fact]
        public void GetPaymentMethods()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var testCredential = new ResursCredential(appSettings["ResursBankUserName"], appSettings["ResursBankUserNamePassword"]);
            var resursBankServices = new ResursBankServiceClient(testCredential);
            List<PaymentMethodResponse> list = resursBankServices.GetPaymentMethods("sv", "NATURAL", 1000);
            Assert.NotNull(list);
        }

        [Fact]
        public void BookPayment()
        {
            var credential = new ResursCredential();
            var appSettings = ConfigurationManager.AppSettings;
            credential.UserName = appSettings["ResursBankUserName"] ?? "Not Found";
            credential.Password = appSettings["ResursBankUserNamePassword"] ?? "Not Found";
            var resursBankServiceClient = new ResursBankServiceClient(credential);
            List<SpecLine> specLines = new List<SpecLine>
            {
                new SpecLine("1","2","description",1,"st",100,25,125,125)
            };
            var customer = new Customer("180872-48794", "+4797674852", "javatest@resurs.se", "NATURAL");
            customer.Address = new Address("Test Testsson", "Test", "Testsson", "Test gatan 25", "abc", "25220", "Test", "SE");
            //var result = resursBankServiceClient.BookPayment("Invoice", "127.0.0.1", specLines, customer, "http://www.google.se", "http://www.google.se", false, "http://www.google.se");
            //Assert.NotNull(result);
        }

        [Fact]
        public void GetAddress()
        {
            var credential = new ResursCredential();
            var appSettings = ConfigurationManager.AppSettings;
            credential.UserName = appSettings["ResursBankUserName"] ?? "Not Found";
            credential.Password = appSettings["ResursBankUserNamePassword"] ?? "Not Found";
            var resursBankServiceClient = new ResursBankServiceClient(credential);
       
            var governmentId = "197812304843";
            var customerType = "LEGAL";
            var customerIpAddress = "127.0.0.1";


            var address = resursBankServiceClient.GetAddress(governmentId, customerType, customerIpAddress);
            Assert.NotNull(address);
        }

        [Fact]
        public void BookSignedPayment()
        {
            var credential = new ResursCredential();
            var appSettings = ConfigurationManager.AppSettings;
            credential.UserName = appSettings["ResursBankUserName"] ?? "Not Found";
            credential.Password = appSettings["ResursBankUserNamePassword"] ?? "Not Found";
            var resursBankServiceClient = new ResursBankServiceClient(credential);

            var paymentId = "1";

            var result = resursBankServiceClient.BookSignedPayment(paymentId);
            Assert.NotNull(result);
        }
    }
}
