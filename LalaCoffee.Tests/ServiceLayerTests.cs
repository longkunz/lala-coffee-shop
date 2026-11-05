using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LalaCoffee.Models;
using LalaCoffee.Services;
using LalaCoffee.Services.Email;
using LalaCoffee.Services.Payment;
using LalaCoffee.Services.Payment.Momo;

namespace LalaCoffee.Tests
{
    [TestClass]
    public class ServiceLayerTests
    {
        #region ICartService / CartService Tests

        [TestMethod]
        public void CartService_AddToCart_NewItem_AddedSuccessfully()
        {
            var cartService = new CartService();
            var item = new CartItem
            {
                Id = Guid.NewGuid(),
                ProductId = 1,
                ProductName = "Cà phê sữa đá",
                Price = 30000m,
                Quantity = 2,
                PropertyProduct = new PropertyProduct { Id = 1, Name = "Size M", Price = 0m },
                Options = new List<OptionProduct>()
            };

            cartService.AddToCart(item);

            var items = cartService.GetCartItems();
            Assert.HasCount(1, items);
            Assert.AreEqual(2, items[0].Quantity);
            Assert.AreEqual("Cà phê sữa đá", items[0].ProductName);
        }

        [TestMethod]
        public void CartService_AddToCart_SameItemAndOptions_IncrementsQuantity()
        {
            var cartService = new CartService();
            var item1 = new CartItem
            {
                Id = Guid.NewGuid(),
                ProductId = 1,
                ProductName = "Cà phê đen",
                Price = 25000m,
                Quantity = 1,
                PropertyProduct = new PropertyProduct { Id = 2, Name = "Size L", Price = 5000m },
                Options = new List<OptionProduct>
                {
                    new OptionProduct { Id = 1, Name = "Boba", Price = 5000m }
                }
            };
            var item2 = new CartItem
            {
                Id = Guid.NewGuid(),
                ProductId = 1,
                ProductName = "Cà phê đen",
                Price = 25000m,
                Quantity = 3,
                PropertyProduct = new PropertyProduct { Id = 2, Name = "Size L", Price = 5000m },
                Options = new List<OptionProduct>
                {
                    new OptionProduct { Id = 1, Name = "Boba", Price = 5000m }
                }
            };

            cartService.AddToCart(item1);
            cartService.AddToCart(item2);

            var items = cartService.GetCartItems();
            Assert.HasCount(1, items);
            Assert.AreEqual(4, items[0].Quantity);
        }

        [TestMethod]
        public void CartService_AddToCart_DifferentOptions_CreatesSeparateItems()
        {
            var cartService = new CartService();
            var item1 = new CartItem
            {
                Id = Guid.NewGuid(),
                ProductId = 1,
                ProductName = "Trà đào",
                Price = 35000m,
                Quantity = 1,
                PropertyProduct = new PropertyProduct { Id = 1, Name = "Size M", Price = 0m },
                Options = new List<OptionProduct>
                {
                    new OptionProduct { Id = 1, Name = "Thạch đào", Price = 5000m }
                }
            };
            var item2 = new CartItem
            {
                Id = Guid.NewGuid(),
                ProductId = 1,
                ProductName = "Trà đào",
                Price = 35000m,
                Quantity = 1,
                PropertyProduct = new PropertyProduct { Id = 1, Name = "Size M", Price = 0m },
                Options = new List<OptionProduct>
                {
                    new OptionProduct { Id = 2, Name = "Trân châu trắng", Price = 5000m }
                }
            };

            cartService.AddToCart(item1);
            cartService.AddToCart(item2);

            var items = cartService.GetCartItems();
            Assert.HasCount(2, items);
        }

        [TestMethod]
        public void CartService_RemoveFromCart_RemovesTargetItem()
        {
            var cartService = new CartService();
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            cartService.AddToCart(new CartItem { Id = id1, ProductId = 1, Quantity = 1 });
            cartService.AddToCart(new CartItem { Id = id2, ProductId = 2, Quantity = 2 });

            cartService.RemoveFromCart(id1);

            var items = cartService.GetCartItems();
            Assert.HasCount(1, items);
            Assert.AreEqual(id2, items[0].Id);
        }

        [TestMethod]
        public void CartService_UpdateCart_UpdatesQuantityCorrectly()
        {
            var cartService = new CartService();
            var id = Guid.NewGuid();
            cartService.AddToCart(new CartItem { Id = id, ProductId = 1, Quantity = 1 });

            var items = cartService.GetCartItems();
            items[0].Quantity = 5;
            cartService.UpdateCart(items);

            var updatedItems = cartService.GetCartItems();
            Assert.AreEqual(5, updatedItems[0].Quantity);
        }

        [TestMethod]
        public void CartService_ClearCart_EmptiesAllItems()
        {
            var cartService = new CartService();
            cartService.AddToCart(new CartItem { Id = Guid.NewGuid(), ProductId = 1, Quantity = 2 });
            cartService.AddToCart(new CartItem { Id = Guid.NewGuid(), ProductId = 2, Quantity = 3 });

            cartService.ClearCart();

            Assert.IsEmpty(cartService.GetCartItems());
        }

        [TestMethod]
        public void CartService_GetCartTotal_CalculatesAccuratelyWithQuantity()
        {
            var cartService = new CartService();
            var cart = new List<CartItem>
            {
                new CartItem
                {
                    ProductId = 1,
                    Price = 30000m,
                    Quantity = 3,
                    PropertyProduct = new PropertyProduct { Price = 5000m },
                    Options = new List<OptionProduct> { new OptionProduct { Price = 5000m } }
                },
                new CartItem
                {
                    ProductId = 2,
                    Price = 40000m,
                    Quantity = 2,
                    PropertyProduct = null,
                    Options = null
                }
            };

            // (30k + 5k + 5k) * 3 = 120k; 40k * 2 = 80k; Total = 200k
            decimal total = cartService.GetCartTotal(cart);
            Assert.AreEqual(200000m, total);
        }

        [TestMethod]
        public void CartService_CompareListOptions_HandlesNullEmptyAndReorderedOptions()
        {
            var cartService = new CartService();

            Assert.IsTrue(cartService.CompareListOptions(null, null));
            Assert.IsFalse(cartService.CompareListOptions(null, new List<OptionProduct>()));
            Assert.IsTrue(cartService.CompareListOptions(new List<OptionProduct>(), new List<OptionProduct>()));

            var opt1 = new List<OptionProduct> { new OptionProduct { Id = 1 }, new OptionProduct { Id = 2 } };
            var opt2 = new List<OptionProduct> { new OptionProduct { Id = 1 }, new OptionProduct { Id = 2 } };
            var opt3 = new List<OptionProduct> { new OptionProduct { Id = 2 }, new OptionProduct { Id = 1 } };

            Assert.IsTrue(cartService.CompareListOptions(opt1, opt2));
            Assert.IsFalse(cartService.CompareListOptions(opt1, opt3));
        }

        #endregion

        #region IOrderService / OrderService Tests

        [TestMethod]
        public void OrderService_GenerateOrderCode_Returns12UppercaseCharacters()
        {
            var orderService = new OrderService((p, d) => { });

            string code1 = orderService.GenerateOrderCode(12);
            string code2 = orderService.GenerateOrderCode(12);

            Assert.AreEqual(12, code1.Length);
            Assert.AreEqual(12, code2.Length);
            Assert.IsTrue(code1.All(c => char.IsLetterOrDigit(c) && (!char.IsLetter(c) || char.IsUpper(c))));
            Assert.AreNotEqual(code1, code2, "Subsequent generated codes should be unique.");
        }

        [TestMethod]
        public void OrderService_ValidatePhoneNumber_ValidVNNumbers_ReturnsTrue()
        {
            var orderService = new OrderService((p, d) => { });

            string[] validNumbers =
            {
                "0987654321",
                "0912345678",
                "0909123456",
                "0356789012",
                "0868123456",
                "+84987654321",
                "+84909123456"
            };

            foreach (var phone in validNumbers)
            {
                Assert.IsTrue(orderService.ValidatePhoneNumber(phone), $"Should be valid VN phone: {phone}");
            }
        }

        [TestMethod]
        public void OrderService_ValidatePhoneNumber_InvalidNumbers_ReturnsFalse()
        {
            var orderService = new OrderService((p, d) => { });

            string[] invalidNumbers =
            {
                "",
                null,
                "123456",
                "01234567890",
                "098765432",    // 9 digits
                "098765432109", // 12 digits
                "abcdefghij",
                "0012345678"
            };

            foreach (var phone in invalidNumbers)
            {
                Assert.IsFalse(orderService.ValidatePhoneNumber(phone), $"Should be invalid phone: {phone}");
            }
        }

        [TestMethod]
        public void OrderService_CreateOrder_CalculatesLineTotalsAndGrandTotalAccurately()
        {
            Order persistedOrder = null;
            List<OrderDetail> persistedDetails = null;

            var orderService = new OrderService((order, details) =>
            {
                persistedOrder = order;
                persistedDetails = details;
            });

            var cart = new List<CartItem>
            {
                new CartItem
                {
                    ProductId = 101,
                    ProductName = "Cà phê muối",
                    Price = 35000m,
                    Quantity = 3,
                    PropertyProduct = new PropertyProduct { Id = 1, Name = "Size L", Price = 8000m },
                    Options = new List<OptionProduct>
                    {
                        new OptionProduct { Id = 1, Name = "Kem béo", Price = 7000m }
                    }
                },
                new CartItem
                {
                    ProductId = 102,
                    ProductName = "Trà sen vàng",
                    Price = 45000m,
                    Quantity = 2,
                    PropertyProduct = new PropertyProduct { Id = 2, Name = "Size M", Price = 0m },
                    Options = new List<OptionProduct>()
                }
            };

            var order = orderService.CreateOrder("Nguyễn Văn A", "0987654321", "123 Đường Lê Lợi", "cash", "Ít đường", "USER_123", cart);

            Assert.IsNotNull(persistedOrder);
            Assert.IsNotNull(persistedDetails);
            Assert.HasCount(2, persistedDetails);

            // Item 1: (35k + 8k + 7k) * 3 = 50k * 3 = 150k
            Assert.AreEqual(150000m, persistedDetails[0].Total);
            Assert.AreEqual(3, persistedDetails[0].Quantity);

            // Item 2: (45k + 0k) * 2 = 90k
            Assert.AreEqual(90000m, persistedDetails[1].Total);
            Assert.AreEqual(2, persistedDetails[1].Quantity);

            // Grand total: 150k + 90k = 240k
            Assert.AreEqual(240000m, order.Total);
            Assert.AreEqual(240000m, persistedOrder.Total);

            // Initial status and payment flags
            Assert.AreEqual("Pending", order.Status);
            Assert.AreEqual(0, order.Paid);
            Assert.AreEqual(12, order.Code.Length);
        }

        [TestMethod]
        public void OrderService_CreateOrder_EmptyCart_ThrowsInvalidOperationException()
        {
            var orderService = new OrderService((p, d) => { });
            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                orderService.CreateOrder("Name", "0987654321", "Address", "cash", "", "USER_1", new List<CartItem>());
            });
        }

        [TestMethod]
        public void OrderService_GetOrderByCode_ReturnsMatchingOrder()
        {
            var existingOrder = new Order { Code = "ORD123456789", Name = "Khách hàng Test", Total = 50000m };
            var orderService = new OrderService(
                orderPersister: (p, d) => { },
                orderLookup: code => code == existingOrder.Code ? existingOrder : null
            );

            var found = orderService.GetOrderByCode("ORD123456789");
            var notFound = orderService.GetOrderByCode("NONEXISTENT");

            Assert.IsNotNull(found);
            Assert.AreEqual("Khách hàng Test", found.Name);
            Assert.IsNull(notFound);
        }

        [TestMethod]
        public void OrderService_UpdateOrderStatus_InvokesUpdaterAndModifiesStatus()
        {
            string updatedCode = null;
            string updatedStatus = null;
            int? updatedPaid = null;

            var orderService = new OrderService(
                orderPersister: (p, d) => { },
                orderLookup: null,
                detailsLookup: null,
                statusUpdater: (code, status, paid) =>
                {
                    updatedCode = code;
                    updatedStatus = status;
                    updatedPaid = paid;
                }
            );

            bool success = orderService.UpdateOrderStatus("ORD999999999", "Paid", 1);

            Assert.IsTrue(success);
            Assert.AreEqual("ORD999999999", updatedCode);
            Assert.AreEqual("Paid", updatedStatus);
            Assert.AreEqual(1, updatedPaid);
        }

        #endregion

        #region IPaymentService / PaymentService Tests

        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

            public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
            {
                _responder = responder;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_responder(request));
            }
        }

        [TestMethod]
        public async Task PaymentService_CreateMomoPaymentUrlAsync_UsesHttpClientAndExtractsPayUrl()
        {
            string capturedRequestContent = null;
            string expectedPayUrl = "https://test-payment.momo.vn/v2/gateway/pay?token=TEST_TOKEN_12345";

            var handler = new FakeHttpMessageHandler(req =>
            {
                capturedRequestContent = req.Content.ReadAsStringAsync().Result;
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent($"{{\"payUrl\": \"{expectedPayUrl}\", \"resultCode\": 0}}", Encoding.UTF8, "application/json")
                };
                return response;
            });

            var httpClient = new HttpClient(handler);
            var config = new Dictionary<string, string>
            {
                { "endPoint_Momo", "https://test-payment.momo.vn/v2/gateway/api/create" },
                { "partnerCode_Momo", "MOMOBKUN20180529" },
                { "accessKey_Momo", "klm05TvNBzhg7h7j" },
                { "secretKey_Momo", "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa" },
                { "redirectUrl_Momo", "https://lalacoffee.vn/Pay/MomoProcessing" },
                { "ipnUrl_Momo", "https://lalacoffee.vn/Pay/MomoIpn" }
            };

            var paymentService = new PaymentService(httpClient, k => config.ContainsKey(k) ? config[k] : null);

            var order = new Order { Code = "ORDER_MOMO_01", Total = 75000m };
            string payUrl = await paymentService.CreateMomoPaymentUrlAsync(order);

            Assert.AreEqual(expectedPayUrl, payUrl);
            Assert.IsNotNull(capturedRequestContent);
            Assert.Contains("MOMOBKUN20180529", capturedRequestContent);
            Assert.Contains("ORDER_MOMO_01", capturedRequestContent);
            Assert.Contains("75000", capturedRequestContent);
        }

        [TestMethod]
        public void PaymentService_ValidateMomoResponse_ValidSignature_ReturnsTrue()
        {
            string secretKey = "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa";
            string accessKey = "klm05TvNBzhg7h7j";

            var config = new Dictionary<string, string>
            {
                { "secretKey_Momo", secretKey },
                { "accessKey_Momo", accessKey }
            };

            var paymentService = new PaymentService(null, k => config.ContainsKey(k) ? config[k] : null);

            var momoResult = new MomoResult
            {
                partnerCode = "MOMO",
                accessKey = accessKey,
                orderId = "ORDER_001",
                amount = "50000",
                orderInfo = "Thanh toan LalaCoffee #ORDER_001",
                orderType = "momo_wallet",
                transId = "123456789",
                message = "Success",
                resultCode = 0,
                payType = "qr",
                responseTime = "2026-11-05 10:00:00",
                extraData = "",
                requestId = "REQ_001"
            };

            string rawHash = MomoSecurityHelper.BuildRawHash(
                accessKey,
                momoResult.amount,
                momoResult.extraData,
                momoResult.message,
                momoResult.orderId,
                momoResult.orderInfo,
                momoResult.orderType,
                momoResult.partnerCode,
                momoResult.payType,
                momoResult.requestId,
                momoResult.responseTime,
                momoResult.resultCode.ToString(),
                momoResult.transId
            );

            momoResult.signature = MomoSecurityHelper.ComputeHmacSha256(rawHash, secretKey);

            Assert.IsTrue(paymentService.ValidateMomoResponse(momoResult));
        }

        [TestMethod]
        public void PaymentService_ValidateMomoResponse_TamperedSignature_ReturnsFalse()
        {
            var config = new Dictionary<string, string>
            {
                { "secretKey_Momo", "secret" },
                { "accessKey_Momo", "access" }
            };

            var paymentService = new PaymentService(null, k => config.ContainsKey(k) ? config[k] : null);

            var momoResult = new MomoResult
            {
                orderId = "ORD_FAKE",
                signature = "invalid_tampered_signature"
            };

            Assert.IsFalse(paymentService.ValidateMomoResponse(momoResult));
        }

        [TestMethod]
        public void PaymentService_CreateVnPayPaymentUrl_BuildsValidUrlWithSortedParamsAndSha512()
        {
            string hashSecret = "OURXWYDKAVHBCFGLCKMLAMYNGVVWLRKR";
            var config = new Dictionary<string, string>
            {
                { "vnp_Url", "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html" },
                { "vnp_ReturnUrl", "https://lalacoffee.vn/Pay/VNPayProcessing" },
                { "vnp_HashSecret", hashSecret },
                { "vnp_TmnCode", "ZAQBCKU5" }
            };

            var paymentService = new PaymentService(null, k => config.ContainsKey(k) ? config[k] : null);

            var order = new Order { Code = "VNPAY_ORD_10", Total = 60000m };
            string paymentUrl = paymentService.CreateVnPayPaymentUrl(order, "192.168.1.1");

            Assert.StartsWith("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?", paymentUrl);
            Assert.Contains("vnp_Amount=6000000", paymentUrl); // 60000 * 100
            Assert.Contains("vnp_TmnCode=ZAQBCKU5", paymentUrl);
            Assert.Contains("vnp_TxnRef=VNPAY_ORD_10", paymentUrl);
            Assert.Contains("vnp_SecureHash=", paymentUrl);
        }

        [TestMethod]
        public void PaymentService_ValidateVnPayResponse_ValidSignature_ReturnsTrue()
        {
            string hashSecret = "OURXWYDKAVHBCFGLCKMLAMYNGVVWLRKR";
            var config = new Dictionary<string, string>
            {
                { "vnp_HashSecret", hashSecret }
            };

            var paymentService = new PaymentService(null, k => config.ContainsKey(k) ? config[k] : null);

            var nvc = new NameValueCollection
            {
                { "vnp_Amount", "6000000" },
                { "vnp_BankCode", "NCB" },
                { "vnp_ResponseCode", "00" },
                { "vnp_TxnRef", "VNPAY_ORD_10" }
            };

            string rawData = VnPaySecurityHelper.BuildRawResponseData(nvc);
            nvc["vnp_SecureHash"] = VnPaySecurityHelper.ComputeHmacSha512(hashSecret, rawData);

            Assert.IsTrue(paymentService.ValidateVnPayResponse(nvc));
        }

        [TestMethod]
        public void PaymentService_ValidateVnPayResponse_InvalidSignature_ReturnsFalse()
        {
            var config = new Dictionary<string, string>
            {
                { "vnp_HashSecret", "secret" }
            };

            var paymentService = new PaymentService(null, k => config.ContainsKey(k) ? config[k] : null);

            var nvc = new NameValueCollection
            {
                { "vnp_Amount", "1000000" },
                { "vnp_SecureHash", "tampered_signature" }
            };

            Assert.IsFalse(paymentService.ValidateVnPayResponse(nvc));
        }

        #endregion

        #region IEmailService Error Isolation & Async Execution

        [TestMethod]
        public void EmailService_SendOrderConfirmation_WhenSmtpFails_DoesNotThrow_IsolatesError()
        {
            var emailService = new EmailService(
                configResolver: k =>
                {
                    if (k == "Email_Sender") return "orders@lalacoffee.vn";
                    if (k == "Email_Password") return "password123";
                    return null;
                },
                messageSender: msg => throw new SmtpException("Simulated SMTP Server Outage")
            );

            // Must NOT throw exception; email failure is completely isolated from checkout
            emailService.SendOrderConfirmation("customer@lalacoffee.vn", "Customer Name", "ORDER_ISO_01", 120000m);
        }

        [TestMethod]
        public async Task EmailService_SendOrderConfirmationAsync_WhenSmtpFails_DoesNotThrow_IsolatesError()
        {
            var emailService = new EmailService(
                configResolver: k =>
                {
                    if (k == "Email_Sender") return "orders@lalacoffee.vn";
                    if (k == "Email_Password") return "password123";
                    return null;
                },
                asyncMessageSender: async msg =>
                {
                    await Task.Yield();
                    throw new TimeoutException("Simulated network timeout during SMTP async dispatch");
                }
            );

            // Must NOT throw exception; async failure is completely isolated
            await emailService.SendOrderConfirmationAsync("customer@lalacoffee.vn", "Customer Name", "ORDER_ISO_02", 85000m);
        }

        [TestMethod]
        public async Task EmailService_SendOrderConfirmationAsync_SendsSuccessfully_WhenValid()
        {
            MailMessage? captured = null;

            var emailService = new EmailService(
                configResolver: k =>
                {
                    if (k == "Email_Sender") return "orders@lalacoffee.vn";
                    if (k == "Email_Password") return "password123";
                    return null;
                },
                asyncMessageSender: async msg =>
                {
                    await Task.Yield();
                    captured = msg;
                }
            );

            await emailService.SendOrderConfirmationAsync("customer@lalacoffee.vn", "Nguyễn Văn B", "ORD_ASYNC_99", 55000m);

            Assert.IsNotNull(captured);
            Assert.AreEqual("customer@lalacoffee.vn", captured.To[0].Address);
            Assert.Contains("ORD_ASYNC_99", captured.Subject);
            Assert.Contains("55,000 đ", captured.Body);
        }

        #endregion
    }
}
