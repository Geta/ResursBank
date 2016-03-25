using System;
using System.Collections.Generic;
using System.Configuration;
using Geta.Resurs.Checkout;
using Geta.Resurs.Checkout.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Integration
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GetPaymentMethods()
        {
            var credential = new ResursCredential();
            var appSettings = ConfigurationManager.AppSettings;
            credential.UserName = appSettings["username"] ?? "Not Found";
            credential.Password = appSettings["password"] ?? "Not Found";
            var resursBankServiceClient = new ResursBankServiceClient(credential);
            resursBankServiceClient.GetPaymentMethods("sv", "NATURAL", 1000);
        }
        [TestMethod]
        public void BookPayment()
        {
            var credential = new ResursCredential();
            var appSettings = ConfigurationManager.AppSettings;
            credential.UserName = appSettings["username"] ?? "Not Found";
            credential.Password = appSettings["password"] ?? "Not Found";
            var resursBankServiceClient = new ResursBankServiceClient(credential);
            List<SpecLine> specLines = new List<SpecLine>
            {
                new SpecLine("1","2","description",1,"st",100,25,125,25)
            };
            var customer = new Customer("7312195873","0988902544", "javatest@resurs.se","NATURAL");
            customer.Address = new Address("Test Testsson", "Test", "Testsson", "Test gatan 25","abc", "25220", "Test", "SE");
            //customer.Address.FullName = "Test Testsson";
            //customer.Address.FirstName = "Test";
            //customer.Address.LastName = "Testsson";
            //customer.Address.AddressRow1 = "Test gatan 25";
            //customer.Address.PostalArea = "25220";
            //customer.Address.PostalCode = "Test";
            //customer.Address.Country = "SE";
            resursBankServiceClient.BookPayment("Invoice", "127.0.0.1", specLines, customer, "http://www.google.se",
                "http://www.google.se", false, "http://www.google.se");
        }
    }
}
