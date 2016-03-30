using System;
using System.Text;
using System.Collections.Generic;
using System.Configuration;
using EPiServer.ServiceLocation;
using Geta.Resurs.Checkout;
using Geta.Resurs.Checkout.AfterShopFlowService;
using Geta.Resurs.Checkout.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Integration
{
    /// <summary>
    /// Summary description for ResursBankServiceClientTest
    /// </summary>
    [TestClass]
    public class ResursBankServiceClientTest
    {
        public ResursBankServiceClientTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void GetPaymentMethods()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var testCredential = new ResursCredential(appSettings["username"], appSettings["password"]);
            var resursBankServices = new ResursBankServiceClient(testCredential);
            List<PaymentMethodResponse> list = resursBankServices.GetPaymentMethods("sv", "NATURAL", 1000);
            Assert.IsNotNull(list);
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
                new SpecLine("1","2","description",1,"st",100,25,125,125)
            };
            var customer = new Customer("180872-48794", "+4797674852", "javatest@resurs.se", "NATURAL");
            customer.Address = new Address("Test Testsson", "Test", "Testsson", "Test gatan 25", "abc", "25220", "Test", "SE");
            resursBankServiceClient.BookPayment("Invoice", "127.0.0.1", specLines, customer, "http://www.google.se", "http://www.google.se", false, "http://www.google.se");
        }

        [TestMethod]
        public void GetAddress()
        {
            var credential = new ResursCredential();
            var appSettings = ConfigurationManager.AppSettings;
            credential.UserName = appSettings["username"] ?? "Not Found";
            credential.Password = appSettings["password"] ?? "Not Found";
            var resursBankServiceClient = new ResursBankServiceClient(credential);
       
            var governmentId = "197812304843";
            var customerType = "LEGAL";
            var customerIpAddress = "127.0.0.1";
           
            resursBankServiceClient.GetAddress(governmentId, customerType, customerIpAddress);
        }

        [TestMethod]
        public void BookSignedPayment()
        {
            var credential = new ResursCredential();
            var appSettings = ConfigurationManager.AppSettings;
            credential.UserName = appSettings["username"] ?? "Not Found";
            credential.Password = appSettings["password"] ?? "Not Found";
            var resursBankServiceClient = new ResursBankServiceClient(credential);

            var paymentId = "1";

            resursBankServiceClient.BookSignedPayment(paymentId);
        }
    }
}
