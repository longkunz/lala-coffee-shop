using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using LalaCoffee.Controllers;
using LalaCoffee.Services.Payment;
using LalaCoffee.Services.Payment.Momo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LalaCoffee.Tests
{
    [TestClass]
    public class PaymentSecurityTests
    {
        private const string TestVnpaySecret = "OURXWYDKAVHBCFGLCKMLAMYNGVVWLRKR";
        private const string TestMomoSecret = "sFcbSGRSJjwGxwhhcEktCHWYUuTuPNDB";
        private const string TestMomoAccessKey = "iPXneGmrJH0G8FOP";

        #region VNPay Tests

        [TestMethod]
        public void VNPay_ComputeHmacSha512_KnownTestVector_ProducesExpectedHash()
        {
            string key = "SECRET_KEY";
            string data = "vnp_Amount=1000000&vnp_Command=pay";

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            string expectedHex;
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hash = hmac.ComputeHash(dataBytes);
                var sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                expectedHex = sb.ToString();
            }

            string actualHex = VnPaySecurityHelper.ComputeHmacSha512(key, data);
            Assert.AreEqual(expectedHex, actualHex);
        }

        [TestMethod]
        public void VNPay_ValidateResponseSignature_WithValidSignature_ReturnsTrue()
        {
            var nvc = new NameValueCollection
            {
                { "vnp_Amount", "10000000" },
                { "vnp_BankCode", "NCB" },
                { "vnp_BankTranNo", "VNP14073809" },
                { "vnp_CardType", "ATM" },
                { "vnp_OrderInfo", "Thanh toan LalaCoffee #123456" },
                { "vnp_PayDate", "20261105090000" },
                { "vnp_ResponseCode", "00" },
                { "vnp_TmnCode", "ZAQBCKU5" },
                { "vnp_TransactionNo", "14073809" },
                { "vnp_TransactionStatus", "00" },
                { "vnp_TxnRef", "ORDER_123456" }
            };

            string rawData = VnPaySecurityHelper.BuildRawResponseData(nvc);
            string expectedHash = VnPaySecurityHelper.ComputeHmacSha512(TestVnpaySecret, rawData);
            nvc["vnp_SecureHash"] = expectedHash;

            bool isValid = VnPaySecurityHelper.ValidateResponseSignature(nvc, TestVnpaySecret);
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void VNPay_ValidateResponseSignature_CaseInsensitiveHash_ReturnsTrue()
        {
            var nvc = new NameValueCollection
            {
                { "vnp_Amount", "5000000" },
                { "vnp_ResponseCode", "00" },
                { "vnp_TxnRef", "ORDER_999" }
            };

            string rawData = VnPaySecurityHelper.BuildRawResponseData(nvc);
            string expectedHash = VnPaySecurityHelper.ComputeHmacSha512(TestVnpaySecret, rawData);
            nvc["vnp_SecureHash"] = expectedHash.ToUpperInvariant();

            bool isValid = VnPaySecurityHelper.ValidateResponseSignature(nvc, TestVnpaySecret);
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void VNPay_ValidateResponseSignature_IgnoresVnpSecureHashType()
        {
            var nvc = new NameValueCollection
            {
                { "vnp_Amount", "5000000" },
                { "vnp_ResponseCode", "00" },
                { "vnp_TxnRef", "ORDER_999" }
            };

            string rawData = VnPaySecurityHelper.BuildRawResponseData(nvc);
            string expectedHash = VnPaySecurityHelper.ComputeHmacSha512(TestVnpaySecret, rawData);

            nvc["vnp_SecureHashType"] = "SHA512";
            nvc["vnp_SecureHash"] = expectedHash;

            bool isValid = VnPaySecurityHelper.ValidateResponseSignature(nvc, TestVnpaySecret);
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void VNPay_ValidateResponseSignature_IgnoresNonVnpKeys()
        {
            var nvc = new NameValueCollection
            {
                { "vnp_Amount", "5000000" },
                { "vnp_ResponseCode", "00" },
                { "vnp_TxnRef", "ORDER_999" }
            };

            string rawData = VnPaySecurityHelper.BuildRawResponseData(nvc);
            string expectedHash = VnPaySecurityHelper.ComputeHmacSha512(TestVnpaySecret, rawData);

            nvc["otherParam"] = "attacker-injected";
            nvc["vnp_SecureHash"] = expectedHash;

            bool isValid = VnPaySecurityHelper.ValidateResponseSignature(nvc, TestVnpaySecret);
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void VNPay_ValidateResponseSignature_TamperedAmount_ReturnsFalse()
        {
            var nvc = new NameValueCollection
            {
                { "vnp_Amount", "10000000" },
                { "vnp_ResponseCode", "00" },
                { "vnp_TxnRef", "ORDER_123456" }
            };

            string rawData = VnPaySecurityHelper.BuildRawResponseData(nvc);
            string validHash = VnPaySecurityHelper.ComputeHmacSha512(TestVnpaySecret, rawData);
            nvc["vnp_SecureHash"] = validHash;

            // Attacker alters amount
            nvc["vnp_Amount"] = "20000000";

            bool isValid = VnPaySecurityHelper.ValidateResponseSignature(nvc, TestVnpaySecret);
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void VNPay_ValidateResponseSignature_TamperedResponseCode_ReturnsFalse()
        {
            var nvc = new NameValueCollection
            {
                { "vnp_Amount", "10000000" },
                { "vnp_ResponseCode", "24" }, // Cancelled
                { "vnp_TxnRef", "ORDER_123456" }
            };

            string rawData = VnPaySecurityHelper.BuildRawResponseData(nvc);
            string validHash = VnPaySecurityHelper.ComputeHmacSha512(TestVnpaySecret, rawData);
            nvc["vnp_SecureHash"] = validHash;

            // Attacker changes response code to 00
            nvc["vnp_ResponseCode"] = "00";

            bool isValid = VnPaySecurityHelper.ValidateResponseSignature(nvc, TestVnpaySecret);
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void VNPay_ValidateResponseSignature_MissingHash_ReturnsFalse()
        {
            var nvc = new NameValueCollection
            {
                { "vnp_Amount", "10000000" },
                { "vnp_ResponseCode", "00" },
                { "vnp_TxnRef", "ORDER_123456" }
            };

            bool isValid = VnPaySecurityHelper.ValidateResponseSignature(nvc, TestVnpaySecret);
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void VNPay_ValidateResponseSignature_EmptyOrNullSecret_ReturnsFalse()
        {
            var nvc = new NameValueCollection
            {
                { "vnp_Amount", "10000000" },
                { "vnp_SecureHash", "somehash" }
            };

            Assert.IsFalse(VnPaySecurityHelper.ValidateResponseSignature(nvc, null));
            Assert.IsFalse(VnPaySecurityHelper.ValidateResponseSignature(nvc, ""));
        }

        [TestMethod]
        public void VNPay_ValidateResponseSignature_NullQueryString_ReturnsFalse()
        {
            Assert.IsFalse(VnPaySecurityHelper.ValidateResponseSignature((NameValueCollection)null!, TestVnpaySecret));
            Assert.IsFalse(VnPaySecurityHelper.ValidateResponseSignature((IDictionary<string, string>)null!, TestVnpaySecret));
        }

        [TestMethod]
        public void VNPay_ValidateResponseSignature_WithDictionary_ReturnsTrue()
        {
            var dict = new Dictionary<string, string>
            {
                { "vnp_Amount", "10000000" },
                { "vnp_BankCode", "NCB" },
                { "vnp_ResponseCode", "00" },
                { "vnp_TxnRef", "ORDER_DICT" }
            };

            string rawData = VnPaySecurityHelper.BuildRawResponseData(dict);
            dict["vnp_SecureHash"] = VnPaySecurityHelper.ComputeHmacSha512(TestVnpaySecret, rawData);

            bool isValid = VnPaySecurityHelper.ValidateResponseSignature(dict, TestVnpaySecret);
            Assert.IsTrue(isValid);
        }

        #endregion

        #region MoMo Tests

        [TestMethod]
        public void MoMo_ComputeHmacSha256_KnownTestVector_ProducesExpectedHash()
        {
            string key = "MOMO_SECRET";
            string message = "accessKey=AK&amount=10000";

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] msgBytes = Encoding.UTF8.GetBytes(message);
            string expectedHex;
            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hash = hmac.ComputeHash(msgBytes);
                expectedHex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }

            string actualHex = MomoSecurityHelper.ComputeHmacSha256(message, key);
            Assert.AreEqual(expectedHex, actualHex);
        }

        [TestMethod]
        public void MoMo_ValidateResponseSignature_WithValidModel_ReturnsTrue()
        {
            var result = new MomoResult
            {
                partnerCode = "MOMOOJOI20210710",
                orderId = "ORDER_MOMO_01",
                requestId = "REQ_01",
                amount = "45000",
                orderInfo = "Thanh toan LalaCoffee #ORDER_MOMO_01",
                orderType = "momo_wallet",
                transId = "TRANS_123456789",
                resultCode = 0,
                message = "Giao dịch thành công.",
                payType = "qr",
                responseTime = "1630000000000",
                extraData = ""
            };

            string rawHash = MomoSecurityHelper.BuildRawHash(
                TestMomoAccessKey,
                result.amount,
                result.extraData,
                result.message,
                result.orderId,
                result.orderInfo,
                result.orderType,
                result.partnerCode,
                result.payType,
                result.requestId,
                result.responseTime,
                result.resultCode.ToString(),
                result.transId
            );

            result.signature = MomoSecurityHelper.ComputeHmacSha256(rawHash, TestMomoSecret);

            bool isValid = MomoSecurityHelper.ValidateResponseSignature(result, TestMomoSecret, TestMomoAccessKey);
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void MoMo_ValidateResponseSignature_WithAccessKeyInModel_ReturnsTrue()
        {
            var result = new MomoResult
            {
                accessKey = TestMomoAccessKey,
                partnerCode = "MOMOOJOI20210710",
                orderId = "ORDER_MOMO_02",
                requestId = "REQ_02",
                amount = "65000",
                orderInfo = "Thanh toan LalaCoffee #ORDER_MOMO_02",
                orderType = "momo_wallet",
                transId = "TRANS_987654321",
                resultCode = 0,
                message = "Giao dịch thành công.",
                payType = "napas",
                responseTime = "1630000001000",
                extraData = "storeId=1"
            };

            string rawHash = MomoSecurityHelper.BuildRawHash(
                result.accessKey,
                result.amount,
                result.extraData,
                result.message,
                result.orderId,
                result.orderInfo,
                result.orderType,
                result.partnerCode,
                result.payType,
                result.requestId,
                result.responseTime,
                result.resultCode.ToString(),
                result.transId
            );

            result.signature = MomoSecurityHelper.ComputeHmacSha256(rawHash, TestMomoSecret);

            // Pass null for accessKey parameter; should use result.accessKey
            bool isValid = MomoSecurityHelper.ValidateResponseSignature(result, TestMomoSecret, null);
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void MoMo_ValidateResponseSignature_WithValidNameValueCollection_ReturnsTrue()
        {
            var nvc = new NameValueCollection
            {
                { "accessKey", TestMomoAccessKey },
                { "amount", "30000" },
                { "extraData", "" },
                { "message", "Success" },
                { "orderId", "ORD_03" },
                { "orderInfo", "LalaCoffee test" },
                { "orderType", "wallet" },
                { "partnerCode", "MOMO" },
                { "payType", "qr" },
                { "requestId", "REQ_03" },
                { "responseTime", "1630000002000" },
                { "resultCode", "0" },
                { "transId", "TRANS_03" }
            };

            string rawHash = MomoSecurityHelper.BuildRawHash(
                nvc["accessKey"],
                nvc["amount"],
                nvc["extraData"],
                nvc["message"],
                nvc["orderId"],
                nvc["orderInfo"],
                nvc["orderType"],
                nvc["partnerCode"],
                nvc["payType"],
                nvc["requestId"],
                nvc["responseTime"],
                nvc["resultCode"],
                nvc["transId"]
            );

            nvc["signature"] = MomoSecurityHelper.ComputeHmacSha256(rawHash, TestMomoSecret);

            bool isValid = MomoSecurityHelper.ValidateResponseSignature(nvc, TestMomoSecret);
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void MoMo_ValidateResponseSignature_WithValidDictionary_ReturnsTrue()
        {
            var dict = new Dictionary<string, string>
            {
                { "accessKey", TestMomoAccessKey },
                { "amount", "40000" },
                { "extraData", "" },
                { "message", "Success" },
                { "orderId", "ORD_04" },
                { "orderInfo", "LalaCoffee test 4" },
                { "orderType", "wallet" },
                { "partnerCode", "MOMO" },
                { "payType", "qr" },
                { "requestId", "REQ_04" },
                { "responseTime", "1630000003000" },
                { "resultCode", "0" },
                { "transId", "TRANS_04" }
            };

            string rawHash = MomoSecurityHelper.BuildRawHash(
                dict["accessKey"],
                dict["amount"],
                dict["extraData"],
                dict["message"],
                dict["orderId"],
                dict["orderInfo"],
                dict["orderType"],
                dict["partnerCode"],
                dict["payType"],
                dict["requestId"],
                dict["responseTime"],
                dict["resultCode"],
                dict["transId"]
            );

            dict["signature"] = MomoSecurityHelper.ComputeHmacSha256(rawHash, TestMomoSecret);

            bool isValid = MomoSecurityHelper.ValidateResponseSignature(dict, TestMomoSecret);
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void MoMo_ValidateResponseSignature_TamperedAmount_ReturnsFalse()
        {
            var result = new MomoResult
            {
                partnerCode = "MOMO",
                orderId = "ORDER_TAMPER",
                requestId = "REQ_TAMPER",
                amount = "10000",
                orderInfo = "Info",
                orderType = "wallet",
                transId = "TR1",
                resultCode = 0,
                message = "Success",
                payType = "qr",
                responseTime = "1630000000000",
                extraData = ""
            };

            string rawHash = MomoSecurityHelper.BuildRawHash(
                TestMomoAccessKey,
                result.amount,
                result.extraData,
                result.message,
                result.orderId,
                result.orderInfo,
                result.orderType,
                result.partnerCode,
                result.payType,
                result.requestId,
                result.responseTime,
                result.resultCode.ToString(),
                result.transId
            );

            result.signature = MomoSecurityHelper.ComputeHmacSha256(rawHash, TestMomoSecret);

            // Tamper amount
            result.amount = "500000";

            bool isValid = MomoSecurityHelper.ValidateResponseSignature(result, TestMomoSecret, TestMomoAccessKey);
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void MoMo_ValidateResponseSignature_TamperedResultCode_ReturnsFalse()
        {
            var result = new MomoResult
            {
                partnerCode = "MOMO",
                orderId = "ORDER_TAMPER_CODE",
                requestId = "REQ_TAMPER_CODE",
                amount = "10000",
                orderInfo = "Info",
                orderType = "wallet",
                transId = "TR2",
                resultCode = 1006, // Transaction denied
                message = "User cancelled",
                payType = "qr",
                responseTime = "1630000000000",
                extraData = ""
            };

            string rawHash = MomoSecurityHelper.BuildRawHash(
                TestMomoAccessKey,
                result.amount,
                result.extraData,
                result.message,
                result.orderId,
                result.orderInfo,
                result.orderType,
                result.partnerCode,
                result.payType,
                result.requestId,
                result.responseTime,
                result.resultCode.ToString(),
                result.transId
            );

            result.signature = MomoSecurityHelper.ComputeHmacSha256(rawHash, TestMomoSecret);

            // Attacker changes resultCode to 0 (success)
            result.resultCode = 0;

            bool isValid = MomoSecurityHelper.ValidateResponseSignature(result, TestMomoSecret, TestMomoAccessKey);
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void MoMo_ValidateResponseSignature_MissingSignature_ReturnsFalse()
        {
            var result = new MomoResult
            {
                partnerCode = "MOMO",
                orderId = "ORDER_NOSIG",
                requestId = "REQ_NOSIG",
                amount = "10000",
                resultCode = 0,
                signature = null
            };

            Assert.IsFalse(MomoSecurityHelper.ValidateResponseSignature(result, TestMomoSecret, TestMomoAccessKey));

            result.signature = "   ";
            Assert.IsFalse(MomoSecurityHelper.ValidateResponseSignature(result, TestMomoSecret, TestMomoAccessKey));
        }

        [TestMethod]
        public void MoMo_ValidateResponseSignature_MissingSecretOrAccessKey_ReturnsFalse()
        {
            var result = new MomoResult
            {
                signature = "somesignature"
            };

            Assert.IsFalse(MomoSecurityHelper.ValidateResponseSignature(result, null, TestMomoAccessKey));
            Assert.IsFalse(MomoSecurityHelper.ValidateResponseSignature(result, "", TestMomoAccessKey));
            Assert.IsFalse(MomoSecurityHelper.ValidateResponseSignature(result, TestMomoSecret, null));
            Assert.IsFalse(MomoSecurityHelper.ValidateResponseSignature(result, TestMomoSecret, ""));
        }

        [TestMethod]
        public void MoMo_ValidateResponseSignature_NullInputs_ReturnsFalse()
        {
            Assert.IsFalse(MomoSecurityHelper.ValidateResponseSignature((MomoResult)null!, TestMomoSecret, TestMomoAccessKey));
            Assert.IsFalse(MomoSecurityHelper.ValidateResponseSignature((NameValueCollection)null!, TestMomoSecret, TestMomoAccessKey));
            Assert.IsFalse(MomoSecurityHelper.ValidateResponseSignature((IDictionary<string, string>)null!, TestMomoSecret, TestMomoAccessKey));
        }

        #endregion

        #region Controller Flow Tests

        [TestMethod]
        public void PayController_VNPayProcessing_InvalidSignature_RedirectsToErrorPayment()
        {
            using (var controller = new PayController())
            {
                var tamperedNvc = new NameValueCollection
                {
                    { "vnp_Amount", "1000000" },
                    { "vnp_ResponseCode", "00" },
                    { "vnp_TxnRef", "ORDER_FAKE" },
                    { "vnp_SecureHash", "invalid_fake_checksum" }
                };

                var result = controller.ProcessVNPay(0, tamperedNvc) as RedirectToRouteResult;

                Assert.IsNotNull(result);
                Assert.AreEqual("ErrorPayment", result.RouteValues["action"]);
                Assert.AreEqual("Pay", result.RouteValues["controller"]);
            }
        }

        [TestMethod]
        public void PayController_VNPayProcessing_MissingSignature_RedirectsToErrorPayment()
        {
            using (var controller = new PayController())
            {
                var nvcWithoutHash = new NameValueCollection
                {
                    { "vnp_Amount", "1000000" },
                    { "vnp_ResponseCode", "00" }
                };

                var result = controller.ProcessVNPay(0, nvcWithoutHash) as RedirectToRouteResult;

                Assert.IsNotNull(result);
                Assert.AreEqual("ErrorPayment", result.RouteValues["action"]);
            }
        }

        [TestMethod]
        public void PayController_VNPayProcessing_NullQueryString_RedirectsToErrorPayment()
        {
            using (var controller = new PayController())
            {
                var result = controller.ProcessVNPay(0, null) as RedirectToRouteResult;

                Assert.IsNotNull(result);
                Assert.AreEqual("ErrorPayment", result.RouteValues["action"]);
            }
        }

        [TestMethod]
        public void PayController_MomoProcessing_InvalidSignature_RedirectsToErrorPayment()
        {
            using (var controller = new PayController())
            {
                var fakeResult = new MomoResult
                {
                    partnerCode = "MOMO",
                    orderId = "FAKE_ORDER",
                    amount = "100000",
                    resultCode = 0,
                    signature = "forged_signature_hash"
                };

                var result = controller.MomoProcessing(fakeResult) as RedirectToRouteResult;

                Assert.IsNotNull(result);
                Assert.AreEqual("ErrorPayment", result.RouteValues["action"]);
                Assert.AreEqual("Pay", result.RouteValues["controller"]);
            }
        }

        [TestMethod]
        public void PayController_MomoProcessing_NullResult_RedirectsToErrorPayment()
        {
            using (var controller = new PayController())
            {
                var result = controller.MomoProcessing(null) as RedirectToRouteResult;

                Assert.IsNotNull(result);
                Assert.AreEqual("ErrorPayment", result.RouteValues["action"]);
            }
        }

        #endregion
    }
}
