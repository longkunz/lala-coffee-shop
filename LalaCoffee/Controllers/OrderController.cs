using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
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

            var model = new CheckOutViewModel();
            if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
            {
                try
                {
                    var userId = User.Identity.GetUserId();
                    model.UserId = userId;
                    var appUser = Context?.Users?.Find(userId);
                    if (appUser != null)
                    {
                        model.Name = appUser.UserName;
                        model.PhoneNumber = appUser.PhoneNumber;
                        model.Email = appUser.Email;
                    }
                    else
                    {
                        model.Name = User.Identity.Name;
                    }
                }
                catch
                {
                    // Guard against unmocked context in unit tests
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut(CheckOutViewModel model)
        {
            if (model == null)
            {
                model = new CheckOutViewModel();
            }

            var cart = _cartService.GetCartItems();
            if (cart == null || cart.Count < 1)
            {
                ModelState.AddModelError(string.Empty, "Giỏ hàng của bạn đang trống.");
            }

            if (!string.IsNullOrWhiteSpace(model.PhoneNumber) && !_orderService.ValidatePhoneNumber(model.PhoneNumber))
            {
                ModelState.AddModelError(nameof(model.PhoneNumber), "Số điện thoại không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                var errorList = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrEmpty(m))
                    .ToList();

                ViewBag.Carts = cart;
                ViewBag.Errors = errorList;
                TempData["Errors"] = errorList;
                return View(model);
            }

            try
            {
                // Persist order and details to database via IOrderService (encapsulating Context.Orders.Add(order) and details)
                Order order = _orderService.CreateOrder(model, cart);

                if (Session != null)
                {
                    Session["orderCode"] = order.Code;
                    Session["order"] = order;
                    Session["email"] = model.Email;
                }

                switch (model.Payment?.ToLowerInvariant())
                {
                    case "momo":
                        return RedirectToAction("MomoPay", "Pay", new { orderCode = order.Code });
                    case "vnpay":
                        return RedirectToAction("VNPay", "Pay", new { orderCode = order.Code });
                    default:
                        _orderService.UpdateOrderStatus(order.Code, "Confirmed");
                        _cartService.ClearCart();
                        if (!string.IsNullOrEmpty(model.Email))
                        {
                            SendEmail(model.Email, order.Name, order.Code);
                        }
                        break;
                }
                return RedirectToAction("CompleteOrder", "Order");
            }
            catch (Exception ex)
            {
                var errorList = new List<string> { ex.Message };
                ViewBag.Carts = cart;
                ViewBag.Errors = errorList;
                TempData["Errors"] = errorList;
                return View(model);
            }
        }

        [NonAction]
        public ActionResult CheckOut(FormCollection formCollection)
        {
            var model = new CheckOutViewModel
            {
                Name = formCollection?["name"] ?? formCollection?["Name"],
                Email = formCollection?["email"] ?? formCollection?["Email"],
                PhoneNumber = formCollection?["phoneNumber"] ?? formCollection?["PhoneNumber"],
                Address = formCollection?["address"] ?? formCollection?["Address"],
                Payment = formCollection?["payment"] ?? formCollection?["Payment"] ?? "cash",
                Note = formCollection?["note"] ?? formCollection?["Note"],
                UserId = formCollection?["UserId"]
            };
            return CheckOut(model);
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
