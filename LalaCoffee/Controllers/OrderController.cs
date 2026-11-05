using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using LalaCoffee.Data;
using LalaCoffee.Models;
using LalaCoffee.Services;
using LalaCoffee.Services.Email;

namespace LalaCoffee.Controllers
{
    public class OrderController : BaseController<Order>
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IEmailService _emailService;

        public OrderController()
            : this(null, null, null)
        {
        }

        public OrderController(IEmailService emailService, CartManager cartManager = null)
            : this(null, (ICartService)cartManager, emailService)
        {
        }

        public OrderController(IOrderService orderService, ICartService cartService, IEmailService emailService)
        {
            _orderService = orderService ?? new OrderService(Context);
            _cartService = cartService ?? new CartService();
            _emailService = emailService ?? new EmailService();
        }

        public static bool ValidateVNPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
            phoneNumber = phoneNumber.Replace("+84", "0").Trim();
            var regex = new Regex(@"^(0)(86|96|97|98|32|33|34|35|36|37|38|39|91|94|83|84|85|81|82|90|93|70|79|77|76|78|92|56|58|99|59|55|87)\d{7}$");
            return regex.IsMatch(phoneNumber);
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray()).ToUpper();
        }

        public void SendEmail(string email, string name, string orderCode)
        {
            _emailService.SendOrderConfirmation(email, name, orderCode);
            ViewBag.Message = "Email sent successfully.";
        }

        // GET: Order
        public ActionResult CheckOut()
        {
            var cart = _cartService.GetCartItems();
            if (cart.Count < 1)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Carts = cart;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut(FormCollection formCollection)
        {
            List<string> errors = new List<string>();
            try
            {
                var name = formCollection["name"];
                var email = formCollection["email"];
                var phoneNumber = formCollection["phoneNumber"];
                var address = formCollection["address"];
                var payment = formCollection["payment"];
                var note = formCollection["note"];
                var userId = formCollection["UserId"];

                if (string.IsNullOrEmpty(name))
                {
                    errors.Add("Vui lòng nhập tên.");
                }

                if (string.IsNullOrEmpty(address))
                {
                    errors.Add("Vui lòng nhập địa chỉ.");
                }

                if (!_orderService.ValidatePhoneNumber(phoneNumber) && !ValidateVNPhoneNumber(phoneNumber))
                {
                    errors.Add("Số điện thoại không hợp lệ.");
                }

                switch (payment)
                {
                    case "cash":
                    case "momo":
                    case "vnpay":
                        break;
                    default:
                        errors.Add("Phương thức thanh toán không hợp lệ.");
                        break;
                }

                var cart = _cartService.GetCartItems();
                if (cart == null || cart.Count < 1)
                {
                    errors.Add("Giỏ hàng của bạn đang trống.");
                }

                if (errors.Count == 0)
                {
                    Order order = new Order
                    {
                        Name = name,
                        PhoneNumber = phoneNumber,
                        Address = address,
                        Payment = payment,
                        Note = note,
                        UserId = userId,
                        Status = "Pending",
                        Paid = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    // Persist order and details to database via IOrderService (encapsulating Context.Orders.Add(order) and details)
                    order = _orderService.CreateOrder(order, cart);

                    if (Session != null)
                    {
                        Session["orderCode"] = order.Code;
                        Session["order"] = order;
                        Session["email"] = email;
                    }

                    switch (payment)
                    {
                        case "momo":
                            return RedirectToAction("MomoPay", "Pay", new { orderCode = order.Code });
                        case "vnpay":
                            return RedirectToAction("VNPay", "Pay", new { orderCode = order.Code });
                        default:
                            _orderService.UpdateOrderStatus(order.Code, "Confirmed");
                            _cartService.ClearCart();
                            if (!string.IsNullOrEmpty(email))
                            {
                                SendEmail(email, order.Name, order.Code);
                            }
                            break;
                    }
                    return RedirectToAction("CompleteOrder", "Order");
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
            TempData["Errors"] = errors;
            return RedirectToAction("CheckOut", "Order");
        }

        [Route("Order/SearchOrder")]
        public ActionResult SearchOrder()
        {
            return View();
        }

        [Route("Order/SearchOrder/{orderCode}")]
        public ActionResult SearchOrder(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode))
            {
                return View();
            }

            var order = _orderService.GetOrderByCode(orderCode);

            if (order != null)
            {
                if (Session != null)
                {
                    Session["orderCode"] = null;
                }
                var orderDetails = _orderService.GetOrderDetails(orderCode);
                ViewBag.OrderDetails = orderDetails;
                return View(order);
            }
            return View();
        }

        public ActionResult CompleteOrder()
        {
            return View();
        }
    }
}
