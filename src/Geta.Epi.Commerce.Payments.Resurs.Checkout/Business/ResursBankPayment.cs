using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Geta.Resurs.Checkout.Model;
using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Business
{
    [Serializable]
    public class ResursBankPayment : OtherPayment
    {
        public string PreferredId { get; set; }
        public string ResursBankPaymentMethodId { get; set; }
        public string CustomerIpAddress { get; set; }
        public bool WaitForFraudControl { get; set; }
        public bool AnnulIfFrozen { get; set; }
        public bool FinalizeIfBooked { get; set; }
        public List<SpecLine> SpecLines { get; set; }
        public Customer Customer { get; set; }
        public string SuccessUrl { get; set; }
        public string FailUrl { get; set; }
        public bool ForceSigning { get; set; }
        public string CallBackUrl { get; set; }
        public string BookingStatus { get; set; }
        public string CardNumber { get; set; }
        public ResursBankPayment() { }

        public ResursBankPayment(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
