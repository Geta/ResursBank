using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geta.Resurs.Checkout.Model
{
    [Serializable]
    public class Card
    {
        public string CardNumber { get; set; }
        public decimal Amount { get; set; }
    }

}
