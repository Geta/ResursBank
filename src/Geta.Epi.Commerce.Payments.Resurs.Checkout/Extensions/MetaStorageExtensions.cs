using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mediachase.Commerce.Storage;

namespace Geta.EPi.Commerce.Payments.Resurs.Checkout.Extensions
{
    public static class MetaStorageExtensions
    {
        public static string GetStringValue(this MetaStorageBase item, string fieldName)
        {
            return item.GetStringValue(fieldName, string.Empty);
        }

        public static string GetStringValue(this MetaStorageBase item, string fieldName, string defaultValue)
        {
            return item[fieldName] != null ? item[fieldName].ToString() : defaultValue;
        }

        public static decimal GetDecimalValue(this MetaStorageBase item, string fieldName, decimal defaultValue)
        {
            if (item[fieldName] != null)
            {
                decimal val = 0;
                if(decimal.TryParse(item[fieldName].ToString(), out val))
                    return val;
            }
            return defaultValue;
        }
    }
}
