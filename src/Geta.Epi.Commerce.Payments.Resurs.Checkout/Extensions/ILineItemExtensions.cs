using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Geta.Resurs.Checkout;
using Geta.Resurs.Checkout.Model;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;

namespace Geta.EPi.Commerce.Payments.Resurs.Checkout.Extensions
{
    public static class ILineItemExtensions
    {
        public static SpecLine ToSpecLineItem(this ILineItem lineItem, IOrderGroup orderGroup, IOrderForm orderForm, IMarketService marketService = null, ITaxCalculator calculator = null)
        {
            if (marketService == null)
                marketService = ServiceLocator.Current.GetInstance<IMarketService>();

            if (calculator == null)
                calculator = ServiceLocator.Current.GetInstance<ITaxCalculator>();

            var market = marketService.GetMarket(orderGroup.MarketId);
            var shipment = orderForm.Shipments.First();

            var unitTax = calculator.GetSalesTax(lineItem, market, shipment.ShippingAddress, new Money(lineItem.PlacedPrice, orderGroup.Currency));
            var unitPrice = orderGroup.PricesIncludeTax ? lineItem.PlacedPrice - unitTax.Amount : lineItem.PlacedPrice;

            var extendedPrice = lineItem.GetExtendedPrice(orderGroup.Currency);
            var extendedSalesTax = calculator.GetSalesTax(lineItem, market, shipment.ShippingAddress, extendedPrice);

            var vatPercent = unitTax.Amount * 100 / unitPrice;

            var totalWithoutVat = orderGroup.PricesIncludeTax ? extendedPrice.Amount - extendedSalesTax : extendedPrice.Amount;
            var totalVatAmount = extendedSalesTax.Amount;
            var totalAmount = totalWithoutVat + totalVatAmount;
            var singleItemPriceWithoutTax = totalWithoutVat / lineItem.Quantity;

            // Resurs Bank uses different price and vat formats for checkout and order update
            return new SpecLine(
                lineItem.LineItemId.ToString(),
                lineItem.Code,
                lineItem.DisplayName,
                lineItem.Quantity,
                UnitMeasureType.ST,
                singleItemPriceWithoutTax,
                vatPercent,
                totalVatAmount,
                totalAmount
                );
        }
    }
}