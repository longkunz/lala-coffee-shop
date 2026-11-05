using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using LalaCoffee.Models;
using LalaCoffee.Services.Payment.Momo;

namespace LalaCoffee.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly Func<string, string> _configResolver;

        public PaymentService()
            : this(null, null)
        {
        }

        public PaymentService(HttpClient httpClient, Func<string, string> configResolver = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _configResolver = configResolver;
        }

        private string GetSetting(string key)
        {
            if (_configResolver != null)
            {
                var customVal = _configResolver(key);
                if (!string.IsNullOrEmpty(customVal))
                {
                    return customVal;
                }
            }

            try
            {
                var appSetting = ConfigurationManager.AppSettings[key];
                if (!string.IsNullOrEmpty(appSetting))
                {
                    return appSetting;
                }
            }
            catch
            {
            }

            return Environment.GetEnvironmentVariable(key) ?? string.Empty;
        }

        #region MoMo

        public virtual async Task<string> CreateMomoPaymentUrlAsync(Order order, string returnUrl = null, string ipnUrl = null, string extraData = "")
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            string endPoint = GetSetting("endPoint_Momo");
            string partnerCode = GetSetting("partnerCode_Momo");
            string accessKey = GetSetting("accessKey_Momo");
            string secretKey = GetSetting("secretKey_Momo");
            string redirectUrl = returnUrl ?? GetSetting("redirectUrl_Momo");
            string effectiveIpnUrl = ipnUrl ?? GetSetting("ipnUrl_Momo");

            string requestType = "captureWallet";
            string orderInfo = "Thanh toan LalaCoffee #" + order.Code;
            string amount = Convert.ToInt64(order.Total).ToString();
            string orderId = order.Code;
            string requestId = Guid.NewGuid().ToString();

            string rawHash = "accessKey=" + accessKey +
                "&amount=" + amount +
                "&extraData=" + (extraData ?? "") +
                "&ipnUrl=" + effectiveIpnUrl +
                "&orderId=" + orderId +
                "&orderInfo=" + orderInfo +
                "&partnerCode=" + partnerCode +
                "&redirectUrl=" + redirectUrl +
                "&requestId=" + requestId +
                "&requestType=" + requestType;

            string signature = MomoSecurityHelper.ComputeHmacSha256(rawHash, secretKey);

            var message = new JObject
            {
                { "partnerCode", partnerCode },
                { "partnerName", "Test" },
                { "storeId", "MomoTestStore" },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderId },
                { "orderInfo", orderInfo },
                { "redirectUrl", redirectUrl },
                { "ipnUrl", effectiveIpnUrl },
                { "lang", "en" },
                { "extraData", extraData ?? "" },
                { "requestType", requestType },
                { "signature", signature }
            };

            var content = new StringContent(message.ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endPoint, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var jResponse = JObject.Parse(responseBody);
            return jResponse.GetValue("payUrl")?.ToString();
        }

        public virtual string CreateMomoPaymentUrl(Order order, string returnUrl = null, string ipnUrl = null, string extraData = "")
        {
            return Task.Run(() => CreateMomoPaymentUrlAsync(order, returnUrl, ipnUrl, extraData)).GetAwaiter().GetResult();
        }

        public virtual bool ValidateMomoResponse(MomoResult result)
        {
            string secretKey = GetSetting("secretKey_Momo");
            string accessKey = GetSetting("accessKey_Momo");
            return MomoSecurityHelper.ValidateResponseSignature(result, secretKey, accessKey);
        }

        public virtual bool ValidateMomoResponse(NameValueCollection parameters)
        {
            string secretKey = GetSetting("secretKey_Momo");
            string accessKey = GetSetting("accessKey_Momo");
            return MomoSecurityHelper.ValidateResponseSignature(parameters, secretKey, accessKey);
        }

        #endregion

        #region VNPay

        public virtual string CreateVnPayPaymentUrl(Order order, string ipAddress)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            string vnp_Url = GetSetting("vnp_Url");
            string vnp_ReturnUrl = GetSetting("vnp_ReturnUrl");
            string vnp_HashSecret = GetSetting("vnp_HashSecret");
            string vnp_TmnCode = GetSetting("vnp_TmnCode");

            var vnp_Params = new Dictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", vnp_TmnCode },
                { "vnp_Locale", "vn" },
                { "vnp_CurrCode", "VND" },
                { "vnp_TxnRef", order.Code },
                { "vnp_OrderInfo", "Thanh toan LalaCoffee #" + order.Code },
                { "vnp_OrderType", "topup" },
                { "vnp_Amount", (Convert.ToInt64(order.Total) * 100).ToString() },
                { "vnp_ReturnUrl", vnp_ReturnUrl },
                { "vnp_IpAddr", ipAddress ?? "127.0.0.1" },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") }
            };

            var sortedParams = vnp_Params
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .OrderBy(o => o.Key, StringComparer.Ordinal)
                .ToDictionary(k => k.Key, v => v.Value);

            var data = new StringBuilder();
            foreach (var kv in sortedParams)
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }

            string queryString = data.ToString();
            string baseUrl = vnp_Url.Contains("?") ? vnp_Url + "&" + queryString : vnp_Url + "?" + queryString;

            string signData = queryString;
            if (signData.Length > 0)
            {
                signData = signData.Remove(signData.Length - 1, 1);
            }

            string vnp_SecureHash = VnPaySecurityHelper.ComputeHmacSha512(vnp_HashSecret, signData);
            return baseUrl + "vnp_SecureHash=" + vnp_SecureHash;
        }

        public virtual bool ValidateVnPayResponse(NameValueCollection queryString)
        {
            string hashSecret = GetSetting("vnp_HashSecret");
            return VnPaySecurityHelper.ValidateResponseSignature(queryString, hashSecret);
        }

        #endregion
    }
}
