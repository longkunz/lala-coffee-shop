using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LalaCoffee.Data;
using LalaCoffee.Models;

namespace LalaCoffee.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly Action<Order, List<OrderDetail>> _orderPersister;
        private readonly Func<string, Order> _orderLookup;
        private readonly Func<string, List<OrderDetail>> _detailsLookup;
        private readonly Action<string, string, int?> _statusUpdater;

        public OrderService()
            : this(new ApplicationDbContext())
        {
        }

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Test-friendly constructor for isolated unit tests without database connection
        public OrderService(
            Action<Order, List<OrderDetail>> orderPersister,
            Func<string, Order> orderLookup = null,
            Func<string, List<OrderDetail>> detailsLookup = null,
            Action<string, string, int?> statusUpdater = null)
        {
            _orderPersister = orderPersister;
            _orderLookup = orderLookup;
            _detailsLookup = detailsLookup;
            _statusUpdater = statusUpdater;
        }

        private static readonly Random _random = new Random();
        private static readonly object _randomLock = new object();

        public virtual string GenerateOrderCode(int length = 12)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] result = new char[length];
            lock (_randomLock)
            {
                for (int i = 0; i < length; i++)
                {
                    result[i] = chars[_random.Next(chars.Length)];
                }
            }
            return new string(result);
        }

        public virtual bool ValidatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            string normalized = phoneNumber.Replace("+84", "0").Trim();
            var regex = new Regex(@"^(0)(86|96|97|98|32|33|34|35|36|37|38|39|91|94|83|84|85|81|82|90|93|70|79|77|76|78|92|56|58|99|59|55|87)\d{7}$");
            return regex.IsMatch(normalized);
        }

        public virtual Order CreateOrder(CheckOutViewModel model, IEnumerable<CartItem> cartItems)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            return CreateOrder(model.Name, model.PhoneNumber, model.Address, model.Payment, model.Note, model.UserId, cartItems);
        }

        public virtual Order CreateOrder(string name, string phoneNumber, string address, string payment, string note, string userId, IEnumerable<CartItem> cartItems)
        {
            if (cartItems == null || !cartItems.Any())
            {
                throw new InvalidOperationException("Giỏ hàng của bạn đang trống.");
            }

            var order = new Order
            {
                Code = GenerateOrderCode(12),
                Name = name,
                PhoneNumber = phoneNumber,
                Address = address,
                Payment = payment,
                Note = note,
                UserId = userId,
                Status = OrderStatus.PendingConfirmation,
                Paid = OrderStatus.Unpaid,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            return CreateOrder(order, cartItems);
        }

        public virtual Order CreateOrder(Order order, IEnumerable<CartItem> cartItems)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (cartItems == null || !cartItems.Any())
            {
                throw new InvalidOperationException("Giỏ hàng của bạn đang trống.");
            }

            if (string.IsNullOrEmpty(order.Code))
            {
                order.Code = GenerateOrderCode(12);
            }

            if (string.IsNullOrEmpty(order.Status))
            {
                order.Status = OrderStatus.PendingConfirmation;
            }
            else
            {
                order.Status = OrderStatus.NormalizeStatus(order.Status);
            }

            if (order.CreatedAt == default(DateTime))
            {
                order.CreatedAt = DateTime.Now;
            }
            order.UpdatedAt = DateTime.Now;

            decimal totalOrder = 0m;
            var orderDetails = new List<OrderDetail>();

            foreach (var item in cartItems)
            {
                var propertyPrice = item.PropertyProduct != null ? item.PropertyProduct.Price : 0m;
                var optionPrices = item.Options != null ? item.Options.Select(o => o.Price) : Enumerable.Empty<decimal>();
                var lineTotal = OrderCalculator.CalculateLineTotal(item.Price, propertyPrice, optionPrices, item.Quantity);

                string propertyProduct = item.PropertyProduct != null
                    ? $"{item.PropertyProduct.Name} - {item.PropertyProduct.Price:N0}đ"
                    : "";

                string optionProduct = "";
                if (item.Options != null)
                {
                    foreach (var option in item.Options)
                    {
                        optionProduct += $"{option.Name} - {option.Price:N0}đ\n";
                    }
                }

                var orderDetail = new OrderDetail
                {
                    Order = order,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Total = lineTotal,
                    Quantity = item.Quantity,
                    PropertyProduct = propertyProduct,
                    OptionProduct = optionProduct
                };

                orderDetails.Add(orderDetail);
                totalOrder += lineTotal;
            }

            order.Total = totalOrder;

            if (_orderPersister != null)
            {
                _orderPersister(order, orderDetails);
            }
            else if (_context != null)
            {
                _context.Orders.Add(order);
                foreach (var detail in orderDetails)
                {
                    _context.OrderDetails.Add(detail);
                }
                _context.SaveChanges();
            }

            return order;
        }

        public virtual Order GetOrderByCode(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode)) return null;

            if (_orderLookup != null)
            {
                return _orderLookup(orderCode);
            }

            if (_context != null)
            {
                return _context.Orders.FirstOrDefault(o => o.Code == orderCode);
            }

            return null;
        }

        public virtual Order GetOrderById(int orderId)
        {
            if (_context != null)
            {
                return _context.Orders.Find(orderId);
            }
            return null;
        }

        public virtual List<OrderDetail> GetOrderDetails(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode)) return new List<OrderDetail>();

            if (_detailsLookup != null)
            {
                return _detailsLookup(orderCode);
            }

            if (_context != null)
            {
                return _context.OrderDetails.Where(p => p.Order.Code == orderCode).ToList();
            }

            return new List<OrderDetail>();
        }

        public virtual bool UpdateOrderStatus(string orderCode, string status, int? paid = null)
        {
            if (string.IsNullOrEmpty(orderCode)) return false;

            if (_statusUpdater != null)
            {
                _statusUpdater(orderCode, status, paid);
                return true;
            }

            var order = GetOrderByCode(orderCode);
            if (order == null) return false;

            if (!string.IsNullOrEmpty(status))
            {
                order.Status = OrderStatus.NormalizeStatus(status);
            }
            if (paid.HasValue)
            {
                order.Paid = paid.Value;
            }
            order.UpdatedAt = DateTime.Now;

            if (_context != null)
            {
                _context.SaveChanges();
            }
            return true;
        }

        public virtual void SaveChanges()
        {
            _context?.SaveChanges();
        }
    }
}
