using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geta.Resurs.Checkout.Model
{
    public class Address
    {
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AddressRow1 { get; set; }
        public string AddressRow2 { get; set; }
        public string PostalArea { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }


        public Address(string fullName, string firstName, string lastName, string addressRow1, string addressRow2, string postalArea, string postalCode, string country)
        {
            FullName = fullName;
            FirstName = firstName;
            LastName = lastName;
            AddressRow1 = addressRow1;
            AddressRow2 = addressRow2;
            PostalArea = postalArea;
            PostalCode = postalCode;
            Country = country;
        }

       
    }
    
}
