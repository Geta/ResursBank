using System;
namespace Geta.Resurs.Checkout.Model
{
    [Serializable]
    public class Customer
    {
        public string GovernmentId { get; set; }
        public Address Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }

        public Customer() { }

        public Customer(string governmentId, string phone, string email, string type)
        {
            GovernmentId = governmentId;
            Phone = phone;
            Email = email;
            Type = type;
        }
    }
}