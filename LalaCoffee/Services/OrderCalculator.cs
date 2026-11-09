using LalaCoffee.Models;
using System.Collections.Generic;
using System.Linq;

namespace LalaCoffee.Services
{
	public static class OrderCalculator
	{
		public static decimal CalculateUnitPrice(decimal basePrice, decimal propertyPrice, IEnumerable<decimal> optionPrices)
		{
			decimal total = basePrice + propertyPrice;
			if (optionPrices != null)
			{
				total += optionPrices.Sum();
			}
			return total;
		}

		public static decimal CalculateLineTotal(decimal basePrice, decimal propertyPrice, IEnumerable<decimal> optionPrices, int quantity)
		{
			if (quantity <= 0)
			{
				return 0m;
			}
			return CalculateUnitPrice(basePrice, propertyPrice, optionPrices) * quantity;
		}

		public static decimal CalculateCartTotal(IEnumerable<CartItem> cart)
		{
			if (cart == null)
			{
				return 0m;
			}

			decimal total = 0m;
			foreach (var item in cart)
			{
				if (item == null) continue;
				decimal propertyPrice = item.PropertyProduct != null ? item.PropertyProduct.Price : 0m;
				var optionPrices = item.Options != null ? item.Options.Select(o => o.Price) : Enumerable.Empty<decimal>();
				total += CalculateLineTotal(item.Price, propertyPrice, optionPrices, item.Quantity);
			}
			return total;
		}
	}
}
