using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;

namespace LalaCoffee.Services.Payment
{
    public static class MomoSecurityHelper
    {
        public static string ComputeHmacSha256(string message, string secretKey)
        {
            if (message == null) message = string.Empty;
            if (string.IsNullOrEmpty(secretKey)) return string.Empty;

            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashValue).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string BuildRawHash(
            string accessKey,
            string amount,
            string extraData,
            string message,
            string orderId,
            string orderInfo,
            string orderType,
            string partnerCode,
            string payType,
            string requestId,
            string responseTime,
            string resultCode,
            string transId)
        {
            return $"accessKey={accessKey ?? ""}&amount={amount ?? ""}&extraData={extraData ?? ""}&message={message ?? ""}&orderId={orderId ?? ""}&orderInfo={orderInfo ?? ""}&orderType={orderType ?? ""}&partnerCode={partnerCode ?? ""}&payType={payType ?? ""}&requestId={requestId ?? ""}&responseTime={responseTime ?? ""}&resultCode={resultCode ?? ""}&transId={transId ?? ""}";
        }

        public static bool ValidateResponseSignature(LalaCoffee.Services.Payment.Momo.MomoResult result, string secretKey, string accessKey = null)
        {
            return ValidateResponseSignature(result, secretKey, accessKey, out _);
        }

        public static bool ValidateResponseSignature(LalaCoffee.Services.Payment.Momo.MomoResult result, string secretKey, string accessKey, out string calculatedSignature)
        {
            calculatedSignature = string.Empty;
            if (result == null || string.IsNullOrWhiteSpace(result.signature) || string.IsNullOrEmpty(secretKey))
            {
                return false;
            }

            string effectiveAccessKey = !string.IsNullOrEmpty(accessKey) ? accessKey : result.accessKey;
            if (string.IsNullOrEmpty(effectiveAccessKey))
            {
                return false;
            }

            string rawHash = BuildRawHash(
                effectiveAccessKey,
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

            calculatedSignature = ComputeHmacSha256(rawHash, secretKey);
            return string.Equals(calculatedSignature, result.signature.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool ValidateResponseSignature(NameValueCollection parameters, string secretKey, string accessKey = null)
        {
            return ValidateResponseSignature(parameters, secretKey, accessKey, out _);
        }

        public static bool ValidateResponseSignature(NameValueCollection parameters, string secretKey, string accessKey, out string calculatedSignature)
        {
            calculatedSignature = string.Empty;
            if (parameters == null || string.IsNullOrEmpty(secretKey))
            {
                return false;
            }

            string signature = parameters["signature"];
            if (string.IsNullOrWhiteSpace(signature))
            {
                return false;
            }

            string effectiveAccessKey = !string.IsNullOrEmpty(accessKey) ? accessKey : parameters["accessKey"];
            if (string.IsNullOrEmpty(effectiveAccessKey))
            {
                return false;
            }

            string rawHash = BuildRawHash(
                effectiveAccessKey,
                parameters["amount"],
                parameters["extraData"],
                parameters["message"],
                parameters["orderId"],
                parameters["orderInfo"],
                parameters["orderType"],
                parameters["partnerCode"],
                parameters["payType"],
                parameters["requestId"],
                parameters["responseTime"],
                parameters["resultCode"],
                parameters["transId"]
            );

            calculatedSignature = ComputeHmacSha256(rawHash, secretKey);
            return string.Equals(calculatedSignature, signature.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool ValidateResponseSignature(IDictionary<string, string> parameters, string secretKey, string accessKey = null)
        {
            return ValidateResponseSignature(parameters, secretKey, accessKey, out _);
        }

        public static bool ValidateResponseSignature(IDictionary<string, string> parameters, string secretKey, string accessKey, out string calculatedSignature)
        {
            calculatedSignature = string.Empty;
            if (parameters == null || string.IsNullOrEmpty(secretKey))
            {
                return false;
            }

            string signature;
            if (!parameters.TryGetValue("signature", out signature) || string.IsNullOrWhiteSpace(signature))
            {
                return false;
            }

            string effectiveAccessKey = !string.IsNullOrEmpty(accessKey)
                ? accessKey
                : (parameters.ContainsKey("accessKey") ? parameters["accessKey"] : null);

            if (string.IsNullOrEmpty(effectiveAccessKey))
            {
                return false;
            }

            string GetVal(string key) => parameters.ContainsKey(key) ? parameters[key] : "";

            string rawHash = BuildRawHash(
                effectiveAccessKey,
                GetVal("amount"),
                GetVal("extraData"),
                GetVal("message"),
                GetVal("orderId"),
                GetVal("orderInfo"),
                GetVal("orderType"),
                GetVal("partnerCode"),
                GetVal("payType"),
                GetVal("requestId"),
                GetVal("responseTime"),
                GetVal("resultCode"),
                GetVal("transId")
            );

            calculatedSignature = ComputeHmacSha256(rawHash, secretKey);
            return string.Equals(calculatedSignature, signature.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
