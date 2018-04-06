using System.Security.Cryptography;
using Geta.Epi.Commerce.Payments.Resurs.Checkout.Callbacks;
using Xunit;

namespace Test.Integration
{
    public class ResursHashCalculatorTest
    {
        [Fact]
        public void ExampleFromDocs()
        {
            var salt = "iCanHasCheezeburger";
            var paymentId = "lePayment";
            var hashCalculator = new ResursHashCalculator(new MD5CryptoServiceProvider());
            
            var result = hashCalculator.Compute(new CallbackData
            {
                PaymentId = paymentId
            }, salt);

            Assert.Equal("ED3381936CCAA2659CF3089F4AA83007", result);
        }
    }
}
