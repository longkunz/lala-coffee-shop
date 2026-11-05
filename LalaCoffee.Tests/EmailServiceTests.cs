using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LalaCoffee.Services.Email;

namespace LalaCoffee.Tests
{
    [TestClass]
    public class EmailServiceTests
    {
        private static string GetProjectDirectory()
        {
            var testAssemblyDir = Path.GetDirectoryName(typeof(EmailServiceTests).Assembly.Location)
                ?? AppDomain.CurrentDomain.BaseDirectory;
            return Path.GetFullPath(Path.Combine(testAssemblyDir, @"..\..\..\..\LalaCoffee"));
        }

        [TestMethod]
        public void EmailService_DefaultValues_AreCorrect()
        {
            var service = new EmailService(_ => null);

            Assert.AreEqual("smtp.gmail.com", service.Host);
            Assert.AreEqual(587, service.Port);
            Assert.IsTrue(service.EnableSsl);
            Assert.AreEqual("LalaCoffee", service.DisplayName);
            Assert.AreEqual(string.Empty, service.FromAddress);
            Assert.AreEqual(string.Empty, service.FromPassword);
        }

        [TestMethod]
        public void EmailService_CustomResolver_OverridesDefaults()
        {
            var config = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Email_Sender", "orders@lalacoffee.vn" },
                { "Email_Password", "custom-password-123" },
                { "Email_Host", "smtp.custommail.com" },
                { "Email_Port", "465" },
                { "Email_EnableSsl", "false" },
                { "Email_DisplayName", "Lala Coffee Roasters" }
            };

            var service = new EmailService(k => config.ContainsKey(k) ? config[k] : null);

            Assert.AreEqual("orders@lalacoffee.vn", service.FromAddress);
            Assert.AreEqual("custom-password-123", service.FromPassword);
            Assert.AreEqual("smtp.custommail.com", service.Host);
            Assert.AreEqual(465, service.Port);
            Assert.IsFalse(service.EnableSsl);
            Assert.AreEqual("Lala Coffee Roasters", service.DisplayName);
        }

        [TestMethod]
        public void EmailService_FallbackToEmailAddressKey_WhenEmailSenderMissing()
        {
            var config = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Email_Address", "fallback@lalacoffee.vn" }
            };

            var service = new EmailService(k => config.ContainsKey(k) ? config[k] : null);

            Assert.AreEqual("fallback@lalacoffee.vn", service.FromAddress);
        }

        [TestMethod]
        public void EmailService_FallbackToEnvironmentVariable_WhenAppSettingsMissing()
        {
            string envVarName = "Email_Sender_UnitTest_" + Guid.NewGuid().ToString("N");
            string testEmail = "envtest@lalacoffee.vn";

            try
            {
                Environment.SetEnvironmentVariable(envVarName, testEmail);

                string resolved = EmailService.GetSetting(envVarName, _ => null);
                Assert.AreEqual(testEmail, resolved);
            }
            finally
            {
                Environment.SetEnvironmentVariable(envVarName, null);
            }
        }

        [TestMethod]
        public void EmailService_SendEmail_InvokesDeliveryWithExpectedMessage()
        {
            MailMessage? sentMessage = null;

            var service = new EmailService(
                k => k == "Email_Sender" ? "noreply@lalacoffee.vn" : null,
                messageSender: msg => sentMessage = msg
            );

            service.SendEmail("customer@example.com", "Test Subject", "<b>Hello World</b>", isHtml: true);

            Assert.IsNotNull(sentMessage);
            Assert.AreEqual("customer@example.com", sentMessage.To[0].Address);
            Assert.AreEqual("noreply@lalacoffee.vn", sentMessage.From?.Address);
            Assert.AreEqual("LalaCoffee", sentMessage.From?.DisplayName);
            Assert.AreEqual("Test Subject", sentMessage.Subject);
            Assert.AreEqual("<b>Hello World</b>", sentMessage.Body);
            Assert.IsTrue(sentMessage.IsBodyHtml);
        }

        [TestMethod]
        public async Task EmailService_SendEmailAsync_InvokesDelivery()
        {
            MailMessage? sentMessage = null;

            var service = new EmailService(
                k => k == "Email_Sender" ? "noreply@lalacoffee.vn" : null,
                asyncMessageSender: msg =>
                {
                    sentMessage = msg;
                    return Task.CompletedTask;
                }
            );

            await service.SendEmailAsync("async@example.com", "Async Subject", "Async Body", isHtml: false);

            Assert.IsNotNull(sentMessage);
            Assert.AreEqual("async@example.com", sentMessage.To[0].Address);
            Assert.AreEqual("Async Subject", sentMessage.Subject);
            Assert.AreEqual("Async Body", sentMessage.Body);
            Assert.IsFalse(sentMessage.IsBodyHtml);
        }

        [TestMethod]
        public void EmailService_SendEmail_ThrowsWhenRecipientEmpty()
        {
            var service = new EmailService(k => k == "Email_Sender" ? "noreply@lalacoffee.vn" : null);

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                service.SendEmail("", "Subject", "Body");
            });
        }

        [TestMethod]
        public void EmailService_SendEmail_ThrowsWhenSenderMissing()
        {
            var service = new EmailService(_ => null);

            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                service.SendEmail("recipient@example.com", "Subject", "Body");
            });
        }

        [TestMethod]
        public void EmailService_SendOrderConfirmation_BuildsSubjectAndBody()
        {
            MailMessage? sentMessage = null;

            var service = new EmailService(
                k =>
                {
                    if (k == "Email_Sender") return "orders@lalacoffee.vn";
                    if (k == "Email_Password") return "secret_pass";
                    return null;
                },
                messageSender: msg => sentMessage = msg
            );

            service.SendOrderConfirmation("customer@gmail.com", "Nguyen Van A", "ORD-12345", 95000);

            Assert.IsNotNull(sentMessage);
            Assert.AreEqual("customer@gmail.com", sentMessage.To[0].Address);
            StringAssert.Contains(sentMessage.Subject, "ORD-12345");
            StringAssert.Contains(sentMessage.Body, "ORD-12345");
            StringAssert.Contains(sentMessage.Body, "95,000");
            StringAssert.Contains(sentMessage.Body, "https://lalacoffee.vn/Order/SearchOrder/ORD-12345");
        }

        [TestMethod]
        public void SourceFiles_DoNotContainHardcodedGmailAppPassword()
        {
            var projectDir = GetProjectDirectory();
            Assert.IsTrue(Directory.Exists(projectDir), $"Project dir must exist: {projectDir}");

            const string leakedPassword = "phhrqqdawnyalkpr";
            const string leakedEmail = "hphcplnh7@gmail.com";

            var filesToCheck = new[]
            {
                Path.Combine(projectDir, @"Controllers\OrderController.cs"),
                Path.Combine(projectDir, @"Controllers\PayController.cs"),
                Path.Combine(projectDir, @"Web.config")
            };

            foreach (var file in filesToCheck)
            {
                Assert.IsTrue(File.Exists(file), $"File must exist: {file}");
                string content = File.ReadAllText(file);

                StringAssert.DoesNotMatch(content, new System.Text.RegularExpressions.Regex(System.Text.RegularExpressions.Regex.Escape(leakedPassword)));
                StringAssert.DoesNotMatch(content, new System.Text.RegularExpressions.Regex(System.Text.RegularExpressions.Regex.Escape(leakedEmail)));
            }
        }

        [TestMethod]
        public void WebConfig_UsesSecretsConfigFile_AndHasNoLeakedPaymentKeys()
        {
            var projectDir = GetProjectDirectory();
            string webConfigPath = Path.Combine(projectDir, "Web.config");
            string examplePath = Path.Combine(projectDir, "secrets.config.example");

            Assert.IsTrue(File.Exists(webConfigPath), "Web.config must exist.");
            Assert.IsTrue(File.Exists(examplePath), "secrets.config.example must exist.");

            string webConfigContent = File.ReadAllText(webConfigPath);

            StringAssert.Contains(webConfigContent, "file=\"secrets.config\"");

            // Leaked payment keys from original repository
            string[] leakedSecrets =
            {
                "sFcbSGRSJjwGxwhhcEktCHWYUuTuPNDB",
                "iPXneGmrJH0G8FOP",
                "OURXWYDKAVHBCFGLCKMLAMYNGVVWLRKR"
            };

            foreach (var secret in leakedSecrets)
            {
                StringAssert.DoesNotMatch(webConfigContent, new System.Text.RegularExpressions.Regex(System.Text.RegularExpressions.Regex.Escape(secret)));
            }

            // Must define Email settings keys
            string[] requiredEmailKeys = { "Email_Sender", "Email_Password", "Email_Host", "Email_Port" };
            foreach (var key in requiredEmailKeys)
            {
                StringAssert.Contains(webConfigContent, $"key=\"{key}\"");
            }
        }
    }
}
