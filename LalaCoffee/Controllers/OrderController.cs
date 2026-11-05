using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using LalaCoffee.Data;
using LalaCoffee.Models;
using LalaCoffee.Services;
using LalaCoffee.Services.Email;
using Unity.Policy;
using System.Web.Helpers;

namespace LalaCoffee.Controllers
{
    public class OrderController : BaseController<Order>
    {
        private readonly CartManager _cartManager;
        private readonly IEmailService _emailService;
        public OrderController() : this(new EmailService(), new CartManager())
        {
        }
        public OrderController(IEmailService emailService, CartManager cartManager = null)
        {
            _emailService = emailService ?? new EmailService();
            _cartManager = cartManager ?? new CartManager();
        }
        public static bool ValidateVNPhoneNumber(string phoneNumber)
        {
            phoneNumber = phoneNumber.Replace("+84", "0");
            Regex regex = new Regex(@"^(0)(86|96|97|98|32|33|34|35|36|37|38|39|91|94|83|84|85|81|82|90|93|70|79|77|76|78|92|56|58|99|59|55|87)\d{7}$");
            return regex.IsMatch(phoneNumber);
        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
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
            var cart = _cartManager.GetCartItems();
            if(cart.Count() < 1){
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
                var UserId = formCollection["UserId"];

                if (string.IsNullOrEmpty(name))
                {
                    errors.Add("Vui lòng nhập tên.");
                }

                if (string.IsNullOrEmpty(address))
                {
                    errors.Add("Vui lòng nhập địa chỉ.");
                }

                if(ValidateVNPhoneNumber(phoneNumber) != true){
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

                var cart = _cartManager.GetCartItems();
                if (cart == null || cart.Count < 1)
                {
                    errors.Add("Giỏ hàng của bạn đang trống.");
                }

                if (errors.Count == 0)
                {
                    Order order = new Order();
                    
                    string code = RandomString(12);
                    order.Code = code;
                    order.Name = name;
                    order.PhoneNumber = phoneNumber;
                    order.Address = address;
                    order.Payment = payment;
                    order.Note = note;
                    order.UserId = UserId;
                    order.Status = "Pending";
                    order.Paid = 0;
                    order.CreatedAt = DateTime.Now;
                    order.UpdatedAt = DateTime.Now;

                    decimal totalOrder = 0;
                    var orderDetails = new List<OrderDetail>();

                    foreach (var item in cart)
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

                        OrderDetail orderDetail = new OrderDetail
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

                    // Lưu đơn hàng và chi tiết đơn hàng vào Database trước khi chuyển sang cổng thanh toán
                    Context.Orders.Add(order);
                    foreach (var detail in orderDetails)
                    {
                        Context.OrderDetails.Add(detail);
                    }
                    Context.SaveChanges();

                    Session["orderCode"] = code;
                    Session["order"] = order;
                    Session["email"] = email;

                    switch (payment)
                    {
                        case "momo":
                            return RedirectToAction("MomoPay", "Pay", new { orderCode = order.Code });
                        case "vnpay":
                            return RedirectToAction("VNPay", "Pay", new { orderCode = order.Code });
                        default:
                            order.Status = "Confirmed";
                            Context.SaveChanges();
                            _cartManager.ClearCart();
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

            if (orderCode == null)
            {
                return View();
            }
            var order = Context.Orders.FirstOrDefault(p => p.Code == orderCode); 
            //var find =  from o in Context.Orders join od in Context.OrderDetails on o.Id equals od.Id where o.Code== orderCode
            //            select o;


            if (order != null)
            {
                // Xóa OrderCode
                Session["orderCode"] = null;
                var orderDetails = Context.OrderDetails.Where(p => p.Order.Code == orderCode).ToList();
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
