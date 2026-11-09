using LalaCoffee.Models;
using System;
using System.Collections.Generic;

namespace LalaCoffee.Services
{
	public interface ICartService
	{
		List<CartItem> GetCartItems();
		void AddToCart(CartItem item);
		void UpdateCart(IEnumerable<CartItem> cart);
		void RemoveFromCart(Guid cartItemId);
		decimal GetCartTotal(IEnumerable<CartItem> cart);
		decimal GetTotal();
		void ClearCart();
		bool CompareListOptions(List<OptionProduct> list1, List<OptionProduct> list2);
	}
}
