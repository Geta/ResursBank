using System.Collections.Generic;
using EPiServer.ServiceLocation;

namespace Geta.Resurs.Checkout
{
    [ServiceConfiguration(typeof(ICountryCodeDictionary))]
    public class CountryCodeDictionary : ICountryCodeDictionary
    {
        public IDictionary<string, string> GetCountryMap()
        {
            return GlobalizationConstants.ThreeLetterCountryCodeMappings;
        }
    }
}