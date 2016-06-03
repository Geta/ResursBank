using System;
using System.ComponentModel.DataAnnotations;


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

        public CustomerType? CustomerTypeField { get; set; }

        public string SpecificTypeField { get; set; }
        public PaymentMethodResponse(string id, string descriptionField, decimal minLimitField, decimal maxLimitField,string specificTypeField, WebLink[] legalInfoLinksField)
        {
            Id = id;
            DescriptionField = descriptionField;
            MinLimitField = minLimitField;
            MaxLimitField = maxLimitField;
            SpecificTypeField = specificTypeField;
            LegalInfoLinksField = legalInfoLinksField;
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
