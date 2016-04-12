using System;
namespace Geta.Resurs.Checkout.Model
{
    [Serializable]
    public class Card
    {
        public string CardNumber { get; set; }
        public decimal Amount { get; set; }
    }
}
