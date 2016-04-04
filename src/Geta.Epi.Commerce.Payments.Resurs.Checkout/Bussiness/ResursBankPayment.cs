using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Geta.Resurs.Checkout.Model;
using Mediachase.Commerce.Orders;
using Mediachase.MetaDataPlus.Configurator;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Bussiness
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
        public string PaymentId { get; set; }
        public string CardNumber { get; set; }
        public ResursBankPayment(){}

        public ResursBankPayment(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
