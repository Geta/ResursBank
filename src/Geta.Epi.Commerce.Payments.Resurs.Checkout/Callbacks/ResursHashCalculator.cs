using System.Security.Cryptography;
using System.Text;
using EPiServer.ServiceLocation;
using StructureMap;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Callbacks
{
    [ServiceConfiguration(typeof(IResursHashCalculator))]
    public class ResursHashCalculator : IResursHashCalculator
    {
        private readonly HashAlgorithm _algorithm;

        [DefaultConstructor]
        public ResursHashCalculator()
        {
            _algorithm = new SHA1CryptoServiceProvider();
        }

        public ResursHashCalculator(HashAlgorithm algorithm)
        {
            _algorithm = algorithm;
        }

        public string Compute(CallbackData parameters, string salt)
        {
            var textBytes = GetTextBytes(parameters, salt);
            var hashBytes = _algorithm.ComputeHash(textBytes);

            var sb = new StringBuilder();
            foreach (var t in hashBytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        protected virtual byte[] GetTextBytes(CallbackData parameters, string salt)
        {
            return Encoding.Default.GetBytes($"{parameters.PaymentId}{salt}");
        }
    }
}