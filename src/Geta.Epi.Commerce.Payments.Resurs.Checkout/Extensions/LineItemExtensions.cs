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
            var discountPerItem = (lineItem.LineItemDiscountAmount + lineItem.OrderLevelDiscountAmount) / lineItem.Quantity;
            //var discountRate = discountPerItem * 100 / lineItem.ListPrice;

            var vatPercent = lineItem.GetDecimalValue(ResursConstants.VatPercent, 0);

            // Resurs Bank uses different price and vat formats for checkout and order update

            return new SpecLine(
                lineItem.Id.ToString(),
                lineItem.Code,
                lineItem.Description,
                lineItem.Quantity,
                "st",//hard code
                lineItem.ListPrice,//hard code
                vatPercent,//hard code
                vatPercent,//hard code
                lineItem.ListPrice //hard code
                );


        }

    }
}