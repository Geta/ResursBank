using System;

namespace Geta.Resurs.Checkout.Model
{
    [Serializable]
    public class Address
    {
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AddressRow1 { get; set; }
        public string AddressRow2 { get; set; }
        public string PostalArea { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }

        public Address()
        {

        }

        public Address(string fullName, string firstName, string lastName, string addressRow1, string addressRow2, string postalArea, string postalCode, string countryCode)
        {
            FullName = fullName;
            FirstName = firstName;
            LastName = lastName;
            AddressRow1 = addressRow1;
            AddressRow2 = addressRow2;
            PostalArea = postalArea;
            PostalCode = postalCode;
            CountryCode = countryCode;
        }
    }
}
