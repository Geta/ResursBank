﻿using System;
using Geta.Epi.Commerce.Payments.Resurs.Checkout;
using Geta.EPi.Commerce.Payments.Resurs.Checkout.Extensions;
using Geta.Resurs.Checkout.Model;
using Mediachase.Commerce.Orders;

namespace Geta.EPi.Commerce.Payments.Resurs.Checkout.Extensions
{
    public static class LineItemExtensions
    {
        public static SpecLine ToSpecLineItem(this LineItem lineItem)
        {
            var vatPercent = lineItem.GetDecimalValue(ResursConstants.VatPercent, 0);

            var unitAmountWithoutVat = lineItem.ListPrice;
            var totalVatAmount = (lineItem.Quantity * unitAmountWithoutVat * vatPercent) / 100;
            var totalWithoutVat = lineItem.Quantity * unitAmountWithoutVat;
            var totalAmount = totalWithoutVat + totalVatAmount;
            // Resurs Bank uses different price and vat formats for checkout and order update
            return new SpecLine(
                lineItem.Id.ToString(),
                lineItem.Code,
                lineItem.Description,
                lineItem.Quantity,
                "st",//hard code
                lineItem.ListPrice,
                vatPercent,//hard code
                totalVatAmount,
                totalAmount
                );


        }

    }
}