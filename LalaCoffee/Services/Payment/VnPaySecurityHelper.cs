using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace LalaCoffee.Services.Payment
{
    public static class VnPaySecurityHelper
    {
        public static string ComputeHmacSha512(string key, string inputData)
        {
            if (string.IsNullOrEmpty(key) || inputData == null)
            {
                return string.Empty;
            }

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                var hash = new StringBuilder(hashValue.Length * 2);
                foreach (byte b in hashValue)
                {
                    hash.Append(b.ToString("x2"));
                }
                return hash.ToString();
            }
        }

        public static string BuildRawResponseData(NameValueCollection queryString)
        {
            if (queryString == null || queryString.Count == 0)
            {
                return string.Empty;
            }

            var sortedDict = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (string key in queryString.AllKeys)
            {
                if (string.IsNullOrEmpty(key)) continue;

                if (key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
                {
                    string value = queryString[key];
                    if (!string.IsNullOrEmpty(value))
                    {
                        sortedDict[key] = value;
                    }
                }
            }

            return BuildQueryString(sortedDict);
        }

        public static string BuildRawResponseData(IDictionary<string, string> responseData)
        {
            if (responseData == null || responseData.Count == 0)
            {
                return string.Empty;
            }

            var sortedDict = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var kv in responseData)
            {
                if (string.IsNullOrEmpty(kv.Key)) continue;

                if (kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(kv.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(kv.Key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(kv.Value))
                    {
                        sortedDict[kv.Key] = kv.Value;
                    }
                }
            }

            return BuildQueryString(sortedDict);
        }

        private static string BuildQueryString(SortedDictionary<string, string> sortedDict)
        {
            var data = new StringBuilder();
            foreach (var kv in sortedDict)
            {
                if (data.Length > 0)
                {
                    data.Append('&');
                }
                data.Append(WebUtility.UrlEncode(kv.Key))
                    .Append('=')
                    .Append(WebUtility.UrlEncode(kv.Value));
            }
            return data.ToString();
        }

        public static bool ValidateResponseSignature(NameValueCollection queryString, string hashSecret)
        {
            return ValidateResponseSignature(queryString, hashSecret, out _);
        }

        public static bool ValidateResponseSignature(NameValueCollection queryString, string hashSecret, out string calculatedHash)
        {
            calculatedHash = string.Empty;
            if (queryString == null || string.IsNullOrEmpty(hashSecret))
            {
                return false;
            }

            string secureHash = queryString["vnp_SecureHash"];
            if (string.IsNullOrWhiteSpace(secureHash))
            {
                return false;
            }

            string rawData = BuildRawResponseData(queryString);
            if (string.IsNullOrEmpty(rawData))
            {
                return false;
            }

            calculatedHash = ComputeHmacSha512(hashSecret, rawData);
            return string.Equals(calculatedHash, secureHash.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool ValidateResponseSignature(IDictionary<string, string> responseData, string hashSecret)
        {
            return ValidateResponseSignature(responseData, hashSecret, out _);
        }

        public static bool ValidateResponseSignature(IDictionary<string, string> responseData, string hashSecret, out string calculatedHash)
        {
            calculatedHash = string.Empty;
            if (responseData == null || string.IsNullOrEmpty(hashSecret))
            {
                return false;
            }

            string secureHash;
            if (!responseData.TryGetValue("vnp_SecureHash", out secureHash) || string.IsNullOrWhiteSpace(secureHash))
            {
                return false;
            }

            string rawData = BuildRawResponseData(responseData);
            if (string.IsNullOrEmpty(rawData))
            {
                return false;
            }

            calculatedHash = ComputeHmacSha512(hashSecret, rawData);
            return string.Equals(calculatedHash, secureHash.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
