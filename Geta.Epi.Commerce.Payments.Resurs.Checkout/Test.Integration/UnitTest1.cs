using System;
using Geta.Resurs.Checkout;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Integration
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var resursBankServiceClient = new ResursBankServiceClient();
            resursBankServiceClient.GetPaymentMethods("sv", "NATURAL", 1000);
        }
    }
}
