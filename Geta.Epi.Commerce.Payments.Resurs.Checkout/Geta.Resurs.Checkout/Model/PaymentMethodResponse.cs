using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geta.Resurs.Checkout.Model
{
    public class PaymentMethodResponse
    {
        public string Id { get; set; }
        public string DescriptionField { get; set; }

        public WebLink[] LegalInfoLinksField { get; set; }

        [Range(0, Double.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        public decimal MinLimitField { get; set; }

        [Range(0, Double.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        public decimal MaxLimitField { get; set; }

        public PaymentMethodType TypeField;

        public System.Nullable<CustomerType>[] CustomerTypeField { get; set; }

        public string SpecificTypeField { get; set; }
        public PaymentMethodResponse(string id, string descriptionField, decimal minLimitField, decimal maxLimitField,string specificTypeField)
        {
            Id = id;
            DescriptionField = descriptionField;
            MinLimitField = minLimitField;
            MaxLimitField = maxLimitField;
            SpecificTypeField = specificTypeField;
        }

       
    }

    public enum PaymentMethodType
    {
        INVOICE,
        REVOLVING_CREDIT,
        CARD,
        PAYMENT_PROVIDER
    }

    public enum CustomerType
    {
        LEGAL,
        NATURAL
    }
}
