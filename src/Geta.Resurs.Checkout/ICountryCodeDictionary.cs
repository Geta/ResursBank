using System.Collections.Generic;

namespace Geta.Resurs.Checkout
{
    /// <summary>
    /// Map three letter country code to two letter country code
    /// </summary>
    public interface ICountryCodeDictionary
    {
        IDictionary<string, string> GetCountryMap();
    }
}