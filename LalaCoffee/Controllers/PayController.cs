using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Windows.Input;
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
        private readonly IRepository<Order> _orderRepository;
        private readonly CartManager _cartManager;
        private readonly IEmailService _emailService;

        public PayController() : this(new EmailService(), null, null)
        {
        }

        public PayController(IEmailService emailService, IRepository<Order> orderRepository = null, CartManager cartManager = null)
        {
            _emailService = emailService ?? new EmailService();
            _orderRepository = orderRepository ?? new Repository<Order>(Context);
            _cartManager = cartManager ?? new CartManager();
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
        // GET: Pay
        public ActionResult MomoPay(string orderCode = null)
        {
            if (string.IsNullOrEmpty(orderCode) && Session["orderCode"] != null)
            {
                orderCode = Session["orderCode"].ToString();
            }

            var order = !string.IsNullOrEmpty(orderCode)
                ? Context.Orders.FirstOrDefault(x => x.Code == orderCode)
                : Session["order"] as Order;

            if (order == null)
            {
                // Lỗi không tìm thấy order
                return RedirectToAction("CheckOut", "Order");
            }
            string endPoint = System.Configuration.ConfigurationManager.AppSettings["endPoint_Momo"];
            string partnerCode = System.Configuration.ConfigurationManager.AppSettings["partnerCode_Momo"];
            string accessKey = System.Configuration.ConfigurationManager.AppSettings["accessKey_Momo"];
            string secretKey = System.Configuration.ConfigurationManager.AppSettings["secretKey_Momo"];
            string redirectUrl = System.Configuration.ConfigurationManager.AppSettings["redirectUrl_Momo"];
            string ipnUrl = System.Configuration.ConfigurationManager.AppSettings["ipnUrl_Momo"];
            string requestType = "captureWallet";
            string orderInfo = "Thanh toan LalaCoffee #" + order.Code + "";
            string amount = Convert.ToInt64(order.Total).ToString();
            string orderId = order.Code;
            string requestId = Guid.NewGuid().ToString();
            string extraData = "";

            //Before sign HMAC SHA256 signature
            string rawHash = "accessKey=" + accessKey +
                "&amount=" + amount +
                "&extraData=" + extraData +
                "&ipnUrl=" + ipnUrl +
                "&orderId=" + orderId +
                "&orderInfo=" + orderInfo +
                "&partnerCode=" + partnerCode +
                "&redirectUrl=" + redirectUrl +
                "&requestId=" + requestId +
                "&requestType=" + requestType
                ;

            MomoSecurity crypto = new MomoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, secretKey);

            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "partnerName", "Test" },
                { "storeId", "MomoTestStore" },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderId },
                { "orderInfo", orderInfo },
                { "redirectUrl", redirectUrl },
                { "ipnUrl", ipnUrl },
                { "lang", "en" },
                { "extraData", extraData },
                { "requestType", requestType },
                { "signature", signature }
            };
            string responseFromMomo = PaymentRequest.sendPaymentRequest(endPoint, message.ToString());
            JObject jmessage = JObject.Parse(responseFromMomo);
            return Redirect(jmessage.GetValue("payUrl").ToString());
        }
        public ActionResult MomoProcessing(MomoResult result)
        {
            string secretKey = ConfigurationManager.AppSettings["secretKey_Momo"]
                ?? Environment.GetEnvironmentVariable("secretKey_Momo");
            string accessKey = ConfigurationManager.AppSettings["accessKey_Momo"]
                ?? Environment.GetEnvironmentVariable("accessKey_Momo");

            if (result == null || !MomoSecurityHelper.ValidateResponseSignature(result, secretKey, accessKey))
            {
                return RedirectToAction("ErrorPayment", "Pay");
            }

            //errorCode = 0 : thanh toán thành công (Request.QueryString["errorCode"])
            if (result.resultCode == 0)
            {
                string orderCode = result.orderId;
                var order = !string.IsNullOrEmpty(orderCode)
                    ? Context.Orders.FirstOrDefault(o => o.Code == orderCode)
                    : null;

                if (order == null && Session["orderCode"] != null)
                {
                    string sCode = Session["orderCode"].ToString();
                    order = Context.Orders.FirstOrDefault(o => o.Code == sCode);
                }

                if (order == null)
                {
                    order = Session["order"] as Order;
                }

                if (order != null)
                {
                    order.Paid = 1;
                    order.Status = "Paid";
                    order.UpdatedAt = DateTime.Now;

                    // Fallback: nếu chi tiết đơn hàng chưa được tạo thì tạo với công thức nhân số lượng đúng
                    var existingDetails = Context.OrderDetails.Where(d => d.Order.Id == order.Id).ToList();
                    if (existingDetails.Count == 0)
                    {
                        var cart = _cartManager.GetCartItems();
                        decimal totalOrder = 0;
                        foreach (var item in cart)
                        {
                            var propPrice = item.PropertyProduct != null ? item.PropertyProduct.Price : 0m;
                            var optPrices = item.Options != null ? item.Options.Select(o => o.Price) : Enumerable.Empty<decimal>();
                            var lineTotal = OrderCalculator.CalculateLineTotal(item.Price, propPrice, optPrices, item.Quantity);

                            string propertyProduct = item.PropertyProduct != null ? $"{item.PropertyProduct.Name} - {item.PropertyProduct.Price:N0}đ" : "";
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
                            totalOrder += lineTotal;
                            Context.OrderDetails.Add(orderDetail);
                        }
                        if (totalOrder > 0)
                        {
                            order.Total = totalOrder;
                        }
                    }

                    Context.SaveChanges();
                    _cartManager.ClearCart();

                    string email = Session["email"]?.ToString();
                    if (string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(order.UserId))
                    {
                        var user = Context.Users.Find(order.UserId);
                        if (user != null) email = user.Email;
                    }
                    if (!string.IsNullOrEmpty(email))
                    {
                        SendEmail(email, order.Name, order.Code);
                    }
                    Session["email"] = null;
                    Session["order"] = null;
                    Session["orderCode"] = null;
                }
                return RedirectToAction("CompleteOrder", "Order");
            }
            return RedirectToAction("ErrorPayment", "Pay");
        }
        public static String HmacSHA512(string key, String inputData)
        {
            return VnPaySecurityHelper.ComputeHmacSha512(key, inputData);
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
        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret, Dictionary<string, string> _requestData)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            string queryString = data.ToString();

            baseUrl += "?" + queryString;
            String signData = queryString;
            if (signData.Length > 0)
            {

                signData = signData.Remove(data.Length - 1, 1);
            }
            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);
            baseUrl += "vnp_SecureHash=" + vnp_SecureHash;

            return baseUrl;
        }
        public ActionResult VNPay(string orderCode = null)
        {
            if (string.IsNullOrEmpty(orderCode) && Session["orderCode"] != null)
            {
                orderCode = Session["orderCode"].ToString();
            }

            var order = !string.IsNullOrEmpty(orderCode)
                ? Context.Orders.FirstOrDefault(x => x.Code == orderCode)
                : Session["order"] as Order;

            if (order == null)
            {
                // Lỗi không tìm thấy order
                return RedirectToAction("CheckOut", "Order");
            }
            string vnp_Url = System.Configuration.ConfigurationManager.AppSettings["vnp_Url"];
            string vnp_ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["vnp_ReturnUrl"];
            string vnp_HashSecret = System.Configuration.ConfigurationManager.AppSettings["vnp_HashSecret"];
            string vnp_TmnCode = System.Configuration.ConfigurationManager.AppSettings["vnp_TmnCode"]; // Mã website của merchant trên hệ thống của VNPAY
            var vnp_Params = new Dictionary<string, string>();
            vnp_Params.Add("vnp_Version", "2.1.0");
            vnp_Params.Add("vnp_Command", "pay");
            vnp_Params.Add("vnp_TmnCode", vnp_TmnCode);
            vnp_Params.Add("vnp_Locale", "vn"); //en= English, vn=Tiếng Việt
            vnp_Params.Add("vnp_CurrCode", "VND");
            vnp_Params.Add("vnp_TxnRef", order.Code); // Mã tham chiếu của giao dịch đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày
            vnp_OrderInfo(order, vnp_Params);
            vnp_Params.Add("vnp_OrderType", "topup"); //Mã danh mục hàng hóa.
            string amount = (Convert.ToInt64(order.Total) * 100).ToString();
            vnp_Params.Add("vnp_Amount", amount); // Số tiền thanh toán
            vnp_Params.Add("vnp_ReturnUrl", vnp_ReturnUrl);
            vnp_Params.Add("vnp_IpAddr", GetIpAddress());
            vnp_Params.Add("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            //vnp_Params.Add("vnp_BankCode", "NCB");
            vnp_Params = vnp_Params.OrderBy(o => o.Key).ToDictionary(k => k.Key, v => v.Value);
            String signData = string.Join("&",
            vnp_Params.Where(x => !string.IsNullOrEmpty(x.Value))
            .Select(k => k.Key + "=" + k.Value));
            string paymentUrl = CreateRequestUrl(vnp_Url, vnp_HashSecret, vnp_Params);
            return Redirect(paymentUrl);
        }

        private static void vnp_OrderInfo(Order order, Dictionary<string, string> vnp_Params)
        {
            vnp_Params.Add("vnp_OrderInfo", "Thanh toan LalaCoffee #" + order.Code + "");
        }

        public ActionResult VNPayProcessing(int vnp_ResponseCode, NameValueCollection queryString = null)
        {
            return ProcessVNPay(vnp_ResponseCode, queryString ?? (Request != null ? Request.QueryString : null));
        }

        public ActionResult ProcessVNPay(int vnp_ResponseCode, NameValueCollection queryString)
        {
            string hashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]
                ?? Environment.GetEnvironmentVariable("vnp_HashSecret");

            if (queryString == null || !VnPaySecurityHelper.ValidateResponseSignature(queryString, hashSecret))
            {
                return RedirectToAction("ErrorPayment", "Pay");
            }

            //errorCode = 0 : thanh toán thành công (Request.QueryString["errorCode"])
            if (vnp_ResponseCode == 00)
            {
                string orderCode = queryString["vnp_TxnRef"];
                var order = !string.IsNullOrEmpty(orderCode)
                    ? Context.Orders.FirstOrDefault(o => o.Code == orderCode)
                    : null;

                if (order == null && Session["orderCode"] != null)
                {
                    string sCode = Session["orderCode"].ToString();
                    order = Context.Orders.FirstOrDefault(o => o.Code == sCode);
                }

                if (order == null)
                {
                    order = Session["order"] as Order;
                }

                if (order != null)
                {
                    order.Paid = 1;
                    order.Status = "Paid";
                    order.UpdatedAt = DateTime.Now;

                    // Fallback: nếu chi tiết đơn hàng chưa được tạo thì tạo với công thức nhân số lượng đúng
                    var existingDetails = Context.OrderDetails.Where(d => d.Order.Id == order.Id).ToList();
                    if (existingDetails.Count == 0)
                    {
                        var cart = _cartManager.GetCartItems();
                        decimal totalOrder = 0;
                        foreach (var item in cart)
                        {
                            var propPrice = item.PropertyProduct != null ? item.PropertyProduct.Price : 0m;
                            var optPrices = item.Options != null ? item.Options.Select(o => o.Price) : Enumerable.Empty<decimal>();
                            var lineTotal = OrderCalculator.CalculateLineTotal(item.Price, propPrice, optPrices, item.Quantity);

                            string propertyProduct = item.PropertyProduct != null ? $"{item.PropertyProduct.Name} - {item.PropertyProduct.Price:N0}đ" : "";
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
                            totalOrder += lineTotal;
                            Context.OrderDetails.Add(orderDetail);
                        }
                        if (totalOrder > 0)
                        {
                            order.Total = totalOrder;
                        }
                    }

                    Context.SaveChanges();
                    _cartManager.ClearCart();

                    string email = Session["email"]?.ToString();
                    if (string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(order.UserId))
                    {
                        var user = Context.Users.Find(order.UserId);
                        if (user != null) email = user.Email;
                    }
                    if (!string.IsNullOrEmpty(email))
                    {
                        SendEmail(email, order.Name, order.Code);
                    }
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
