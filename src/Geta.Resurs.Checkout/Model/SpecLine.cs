using System;
namespace Geta.Resurs.Checkout.Model
{
    [Serializable]
    public class SpecLine
    {
        public string Id { get; set; }
        public string ArtNo { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public string UnitMeasure { get; set; }
        public decimal UnitAmountWithoutVat { get; set; }
        public decimal VatPct { get; set; }
        public decimal TotalVatAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public SpecLine(string id, string artNo,string description, decimal quantity, string unitMeasure, decimal unitAmountWithoutVat, decimal vatPct, decimal totalVatAmount, decimal totalAmount)
        {
            Id = id;
            ArtNo = artNo;
            Description = description;
            Quantity = quantity;
            UnitMeasure = unitMeasure;
            UnitAmountWithoutVat = unitAmountWithoutVat;
            VatPct = vatPct;
            TotalVatAmount = totalVatAmount;
            TotalAmount = totalAmount;
        }
    }
}
