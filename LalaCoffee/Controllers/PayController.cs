using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Mvc;
using LalaCoffee.Data;
using LalaCoffee.Models;
using LalaCoffee.Services;
using LalaCoffee.Services.Email;
using LalaCoffee.Services.Payment;
using LalaCoffee.Services.Payment.Momo;

namespace LalaCoffee.Controllers
{
    public class PayController : BaseController<Payment>
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IEmailService _emailService;
        private readonly IRepository<Order> _orderRepository;

        public PayController()
            : this(null, null, null, null)
        {
        }

        public PayController(
            IEmailService emailService,
            IRepository<Order> orderRepository = null,
            CartManager cartManager = null)
            : this(null, null, (ICartService)cartManager, emailService)
        {
            _orderRepository = orderRepository ?? new Repository<Order>(Context);
        }

        public PayController(
            IPaymentService paymentService,
            IOrderService orderService,
            ICartService cartService,
            IEmailService emailService)
        {
            _paymentService = paymentService ?? new PaymentService();
            _orderService = orderService ?? new OrderService(Context);
            _cartService = cartService ?? new CartService();
            _emailService = emailService ?? new EmailService();
            _orderRepository = new Repository<Order>(Context);
        }

        public ActionResult ErrorPayment()
        {
            return View();
        }

        public void SendEmail(string email, string name, string orderCode)
        {
            _emailService.SendOrderConfirmation(email, name, orderCode);
            ViewBag.Message = "Email sent successfully.";
        }

        // GET: Pay/MomoPay
        public ActionResult MomoPay(string orderCode = null)
        {
            if (string.IsNullOrEmpty(orderCode) && Session != null && Session["orderCode"] != null)
            {
                orderCode = Session["orderCode"].ToString();
            }

            var order = !string.IsNullOrEmpty(orderCode)
                ? _orderService.GetOrderByCode(orderCode) // Context.Orders.FirstOrDefault
                : (Session != null ? Session["order"] as Order : null);

            if (order == null)
            {
                return RedirectToAction("CheckOut", "Order");
            }

            string payUrl = _paymentService.CreateMomoPaymentUrl(order);
            return Redirect(payUrl);
        }

        public ActionResult MomoProcessing(MomoResult result)
        {
            if (result == null || !_paymentService.ValidateMomoResponse(result))
            {
                return RedirectToAction("ErrorPayment", "Pay");
            }

            if (result.resultCode == 0)
            {
                string orderCode = result.orderId;
                var order = !string.IsNullOrEmpty(orderCode)
                    ? _orderService.GetOrderByCode(orderCode)
                    : null;

                if (order == null && Session != null && Session["orderCode"] != null)
                {
                    string sCode = Session["orderCode"].ToString();
                    order = _orderService.GetOrderByCode(sCode);
                }

                if (order == null)
                {
                    return RedirectToAction("ErrorPayment", "Pay");
                }

                _orderService.UpdateOrderStatus(order.Code, "Paid", paid: 1);
                _cartService.ClearCart();

                string email = Session != null ? Session["email"]?.ToString() : null;
                if (string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(order.UserId))
                {
                    var user = Context.Users.Find(order.UserId);
                    if (user != null) email = user.Email;
                }
                if (!string.IsNullOrEmpty(email))
                {
                    SendEmail(email, order.Name, order.Code);
                }

                if (Session != null)
                {
                    Session["email"] = null;
                    Session["order"] = null;
                    Session["orderCode"] = null;
                }

                return RedirectToAction("CompleteOrder", "Order");
            }

            return RedirectToAction("ErrorPayment", "Pay");
        }

        public string GetIpAddress()
        {
            string ipAddress;
            try
            {
                ipAddress = HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress) || (ipAddress.ToLower() == "unknown") || ipAddress.Length > 45)
                    ipAddress = HttpContext.Request.ServerVariables["REMOTE_ADDR"];
            }
            catch (Exception ex)
            {
                ipAddress = "Invalid IP:" + ex.Message;
            }

            return ipAddress;
        }

        // GET: Pay/VNPay
        public ActionResult VNPay(string orderCode = null)
        {
            if (string.IsNullOrEmpty(orderCode) && Session != null && Session["orderCode"] != null)
            {
                orderCode = Session["orderCode"].ToString();
            }

            var order = !string.IsNullOrEmpty(orderCode)
                ? _orderService.GetOrderByCode(orderCode) // Context.Orders.FirstOrDefault
                : (Session != null ? Session["order"] as Order : null);

            if (order == null)
            {
                return RedirectToAction("CheckOut", "Order");
            }

            string paymentUrl = _paymentService.CreateVnPayPaymentUrl(order, GetIpAddress());
            return Redirect(paymentUrl);
        }

        public ActionResult VNPayProcessing(int vnp_ResponseCode, NameValueCollection queryString = null)
        {
            return ProcessVNPay(vnp_ResponseCode, queryString ?? (Request != null ? Request.QueryString : null));
        }

        public ActionResult ProcessVNPay(int vnp_ResponseCode, NameValueCollection queryString)
        {
            if (queryString == null || !_paymentService.ValidateVnPayResponse(queryString))
            {
                return RedirectToAction("ErrorPayment", "Pay");
            }

            if (vnp_ResponseCode == 00)
            {
                string orderCode = queryString["vnp_TxnRef"];
                var order = !string.IsNullOrEmpty(orderCode)
                    ? _orderService.GetOrderByCode(orderCode)
                    : null;

                if (order == null && Session != null && Session["orderCode"] != null)
                {
                    string sCode = Session["orderCode"].ToString();
                    order = _orderService.GetOrderByCode(sCode);
                }

                if (order == null)
                {
                    return RedirectToAction("ErrorPayment", "Pay");
                }

                _orderService.UpdateOrderStatus(order.Code, "Paid", paid: 1);
                _cartService.ClearCart();

                string email = Session != null ? Session["email"]?.ToString() : null;
                if (string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(order.UserId))
                {
                    var user = Context.Users.Find(order.UserId);
                    if (user != null) email = user.Email;
                }
                if (!string.IsNullOrEmpty(email))
                {
                    SendEmail(email, order.Name, order.Code);
                }

                if (Session != null)
                {
                    Session["email"] = null;
                    Session["order"] = null;
                    Session["orderCode"] = null;
                }

                return RedirectToAction("CompleteOrder", "Order");
            }

            return RedirectToAction("ErrorPayment", "Pay");
        }
    }
}
