using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LalaCoffee.Models;

namespace LalaCoffee.Services
{
    public class CartService : ICartService
    {
        public const string CartSessionKey = "Cart";
        private readonly Func<HttpSessionStateBase> _sessionProvider;
        private List<CartItem> _inMemoryCart;

        public CartService()
            : this((Func<HttpSessionStateBase>)null)
        {
        }

        public CartService(HttpSessionStateBase session)
            : this(() => session)
        {
        }

        public CartService(Func<HttpSessionStateBase> sessionProvider)
        {
            _sessionProvider = sessionProvider;
        }

        private HttpSessionStateBase GetSession()
        {
            if (_sessionProvider != null)
            {
                return _sessionProvider();
            }

            if (HttpContext.Current?.Session != null)
            {
                return new HttpSessionStateWrapper(HttpContext.Current.Session);
            }

            return null;
        }

        public virtual bool CompareListOptions(List<OptionProduct> list1, List<OptionProduct> list2)
        {
            if (list1 == null && list2 == null) return true;
            if (list1 == null || list2 == null) return false;

            if (list1.Count != list2.Count)
            {
                return false;
            }

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i].Id != list2[i].Id)
                {
                    return false;
                }
            }
            return true;
        }

        public virtual List<CartItem> GetCartItems()
        {
            var session = GetSession();
            if (session != null)
            {
                var cart = session[CartSessionKey] as List<CartItem>;
                if (cart == null)
                {
                    cart = new List<CartItem>();
                    session[CartSessionKey] = cart;
                }
                return cart;
            }

            if (_inMemoryCart == null)
            {
                _inMemoryCart = new List<CartItem>();
            }
            return _inMemoryCart;
        }

        public virtual void AddToCart(CartItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var cart = GetCartItems();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == item.ProductId
                                                     && c.PropertyProduct?.Id == item.PropertyProduct?.Id
                                                     && CompareListOptions(c.Options ?? new List<OptionProduct>(), item.Options ?? new List<OptionProduct>()));
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                cart.Add(item);
            }
        }

        public virtual void UpdateCart(IEnumerable<CartItem> cart)
        {
            var updated = cart != null ? cart.ToList() : new List<CartItem>();
            var session = GetSession();
            if (session != null)
            {
                session[CartSessionKey] = updated;
            }
            else
            {
                _inMemoryCart = updated;
            }
        }

        public virtual void RemoveFromCart(Guid cartItemId)
        {
            var cart = GetCartItems();
            var existingItem = cart.FirstOrDefault(c => c.Id == cartItemId);
            if (existingItem != null)
            {
                cart.Remove(existingItem);
            }
        }

        public virtual decimal GetCartTotal(IEnumerable<CartItem> cart)
        {
            return OrderCalculator.CalculateCartTotal(cart);
        }

        public virtual decimal GetTotal()
        {
            var cart = GetCartItems();
            return OrderCalculator.CalculateCartTotal(cart);
        }

        public virtual void ClearCart()
        {
            var cart = GetCartItems();
            cart.Clear();
        }
    }
}
