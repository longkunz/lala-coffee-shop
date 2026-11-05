using System;
using System.Collections.Generic;
using LalaCoffee.Models;

namespace LalaCoffee.Services
{
    public interface IOrderService
    {
        string GenerateOrderCode(int length = 12);
        bool ValidatePhoneNumber(string phoneNumber);
        Order CreateOrder(Order order, IEnumerable<CartItem> cartItems);
        Order CreateOrder(CheckOutViewModel model, IEnumerable<CartItem> cartItems);
        Order CreateOrder(string name, string phoneNumber, string address, string payment, string note, string userId, IEnumerable<CartItem> cartItems);
        Order GetOrderByCode(string orderCode);
        Order GetOrderById(int orderId);
        List<OrderDetail> GetOrderDetails(string orderCode);
        bool UpdateOrderStatus(string orderCode, string status, int? paid = null);
        void SaveChanges();
    }
}
