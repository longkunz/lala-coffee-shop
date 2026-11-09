using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace LalaCoffee.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly Action<MailMessage> _messageSender;
        private readonly Func<MailMessage, Task> _asyncMessageSender;

        // Timeout chung cho gửi mail: SmtpClient.Timeout chỉ giới hạn Send() đồng bộ,
        // còn đường async (SendMailAsync) phải tự áp deadline bằng giá trị này.
        private const int SmtpTimeoutMilliseconds = 10000;

        // Thời gian chờ thêm sau khi SendAsyncCancel(): tác vụ gửi nền có thể còn
        // đang chờ TCP connect ở mức OS (xem ghi chú trong SendEmailAsync) nên cho
        // nó thêm tối đa ngần này để kết thúc trước khi SmtpClient/MailMessage bị
        // dispose; caller không bao giờ treo theo tác vụ nền quá giới hạn này.
        private const int SmtpCancelGraceMilliseconds = 5000;

        public string Host { get; }
        public int Port { get; }
        public bool EnableSsl { get; }
        public string FromAddress { get; }
        public string FromPassword { get; }
        public string DisplayName { get; }
        public string BaseUrl { get; }

        // Email chỉ được coi là đã cấu hình khi có địa chỉ gửi và mật khẩu thật.
        // Mật khẩu dạng placeholder (ví dụ "DUMMY_..." trong Web.config mẫu) phải được
        // xử lý như chưa cấu hình, nếu không mỗi lần đặt hàng sẽ tốn một lần kết nối
        // SMTP thất bại ngay trong request của khách.
        private bool IsEmailConfigured =>
            !string.IsNullOrWhiteSpace(FromAddress)
            && !string.IsNullOrWhiteSpace(FromPassword)
            && !FromPassword.StartsWith("DUMMY", StringComparison.OrdinalIgnoreCase);

        public EmailService()
            : this(null, null, null)
        {
        }

        public EmailService(
            Func<string, string> configResolver,
            Action<MailMessage> messageSender = null,
            Func<MailMessage, Task> asyncMessageSender = null)
        {
            _messageSender = messageSender;
            _asyncMessageSender = asyncMessageSender;

            FromAddress = GetSetting("Email_Sender", configResolver)
                          ?? GetSetting("Email_Address", configResolver)
                          ?? string.Empty;

            FromPassword = GetSetting("Email_Password", configResolver) ?? string.Empty;

            Host = GetSetting("Email_Host", configResolver) ?? "smtp.gmail.com";

            string portStr = GetSetting("Email_Port", configResolver);
            Port = int.TryParse(portStr, out int port) ? port : 587;

            string sslStr = GetSetting("Email_EnableSsl", configResolver);
            EnableSsl = !bool.TryParse(sslStr, out bool ssl) || ssl;

            DisplayName = GetSetting("Email_DisplayName", configResolver) ?? "LalaCoffee";
            BaseUrl = GetSetting("Email_BaseUrl", configResolver) ?? "https://lalacoffee.vn";
        }

        public static string GetSetting(string key, Func<string, string> customResolver = null)
        {
            if (customResolver != null)
            {
                var customVal = customResolver(key);
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
                // In isolated test environments ConfigurationManager may fail to initialize
            }

            var envVal = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(envVal))
            {
                return envVal;
            }

            return null;
        }

        public virtual MailMessage CreateMailMessage(string toEmail, string subject, string body, bool isHtml = true)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Recipient email address cannot be empty.", nameof(toEmail));

            if (string.IsNullOrWhiteSpace(FromAddress))
                throw new InvalidOperationException("Email sender address is not configured. Please configure 'Email_Sender' or 'Email_Address'.");

            var fromAddress = !string.IsNullOrWhiteSpace(DisplayName)
                ? new MailAddress(FromAddress, DisplayName)
                : new MailAddress(FromAddress);

            var toAddress = new MailAddress(toEmail);

            return new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
        }

        public virtual SmtpClient CreateSmtpClient()
        {
            return new SmtpClient
            {
                Host = Host,
                Port = Port,
                EnableSsl = EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(FromAddress, FromPassword),
                // Không đặt Timeout thì SmtpClient chờ mặc định 100 giây; gửi mail được gọi
                // đồng bộ trong request đặt hàng nên timeout dài sẽ làm treo cả phiên đặt hàng.
                // Lưu ý: Timeout này chỉ giới hạn Send() đồng bộ, không giới hạn SendMailAsync
                // (xem deadline tự áp trong SendEmailAsync).
                Timeout = SmtpTimeoutMilliseconds
            };
        }

        public void SendEmail(string toEmail, string subject, string body, bool isHtml = true)
        {
            using (var message = CreateMailMessage(toEmail, subject, body, isHtml))
            {
                if (_messageSender != null)
                {
                    _messageSender(message);
                    return;
                }

                using (var client = CreateSmtpClient())
                {
                    client.Send(message);
                }
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            using (var message = CreateMailMessage(toEmail, subject, body, isHtml))
            {
                if (_asyncMessageSender != null)
                {
                    await _asyncMessageSender(message).ConfigureAwait(false);
                    return;
                }
                if (_messageSender != null)
                {
                    _messageSender(message);
                    return;
                }

                using (var client = CreateSmtpClient())
                {
                    // SmtpClient.Timeout chỉ giới hạn Send() đồng bộ, KHÔNG giới hạn
                    // SendMailAsync: SMTP không phản hồi sẽ treo quá 10 giây. .NET Framework
                    // 4.8 không có Task.WaitAsync và SendMailAsync không nhận
                    // CancellationToken, nên đua tác vụ gửi với Task.Delay rồi hủy bằng
                    // SendAsyncCancel() khi hết hạn; ném TimeoutException rõ ràng để
                    // caller (SendOrderConfirmationAsync) ghi log thay vì treo lặng lẽ.
                    // Lưu ý: SendAsyncCancel() KHÔNG hủy được lần TCP connect đang
                    // pending — SendAsync → BeginGetConnection, còn Abort() chỉ đóng
                    // pooledStream khi khác null; trong lúc connect đang chờ thì
                    // pooledStream == null nên Abort() chỉ set isClosed=true. Tác vụ
                    // nền vì vậy có thể còn sống tới khi lần connect TCP tự xong/thất
                    // bại ở mức OS (mail không bị gửi muộn: ConnectionCreatedCallback
                    // kiểm tra isClosed, ConnectCallback kiểm tra cancelled trước khi
                    // handshake/gửi). Vì thế sau khi hủy vẫn chờ thêm tối đa
                    // SmtpCancelGraceMilliseconds cho tác vụ nền kết thúc trước khi
                    // rời using (tránh dispose đua với tác vụ đang chạy), rồi mới
                    // ném timeout; caller không bao giờ treo theo tác vụ nền vô hạn.
                    // Timeout ≠ chưa gửi: nếu mail đã gửi xong thì đó là thành công.
                    var sendTask = client.SendMailAsync(message);
                    var timeoutTask = Task.Delay(SmtpTimeoutMilliseconds);
                    var completedTask = await Task.WhenAny(sendTask, timeoutTask).ConfigureAwait(false);
                    if (completedTask == timeoutTask)
                    {
                        // Tác vụ gửi có thể đã hoàn tất đúng lúc hết hạn: nếu đã
                        // xong thì thành công là thành công (không báo timeout sai),
                        // lỗi thì ném lỗi gốc thay vì TimeoutException.
                        if (sendTask.IsCompleted)
                        {
                            await sendTask.ConfigureAwait(false);
                            return;
                        }

                        try
                        {
                            client.SendAsyncCancel();
                        }
                        catch
                        {
                            // Hủy gửi là best-effort; lỗi timeout mới là lỗi cần báo.
                        }

                        // Chờ tác vụ nền kết thúc trong giới hạn grace period trước
                        // khi dispose client/message.
                        var settledTask = await Task.WhenAny(
                            sendTask, Task.Delay(SmtpCancelGraceMilliseconds)).ConfigureAwait(false);
                        if (settledTask == sendTask)
                        {
                            if (sendTask.Status == TaskStatus.RanToCompletion)
                            {
                                // Mail đã gửi xong trong lúc chờ hủy: thành công.
                                await sendTask.ConfigureAwait(false);
                                return;
                            }

                            // Tác vụ kết thúc lỗi/bị hủy sau khi đã hết hạn: lỗi
                            // timeout mới là lỗi cần báo; nuốt lỗi tác vụ nền.
                            try
                            {
                                await sendTask.ConfigureAwait(false);
                            }
                            catch
                            {
                            }
                        }
                        else
                        {
                            Trace.TraceWarning(
                                $"Sending email to '{toEmail}' timed out after {SmtpTimeoutMilliseconds} milliseconds and the background send task did not finish within the additional {SmtpCancelGraceMilliseconds} milliseconds grace period; it may still be waiting on an OS-level TCP connect and its delivery status is unknown (timeout does not mean the email was not sent).");

                            // Quan sát lỗi muộn của tác vụ gửi đã bị hủy để không thành
                            // unobserved exception.
                            _ = sendTask.ContinueWith(
                                t => { var ignored = t.Exception; },
                                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
                        }

                        throw new TimeoutException(
                            $"Sending email to '{toEmail}' timed out after {SmtpTimeoutMilliseconds} milliseconds.");
                    }

                    await sendTask.ConfigureAwait(false);
                }
            }
        }

        public void SendOrderConfirmation(string toEmail, string name, string orderCode, decimal totalAmount = 0)
        {
            if (string.IsNullOrWhiteSpace(toEmail) || !IsEmailConfigured)
            {
                Trace.TraceWarning("SendOrderConfirmation skipped: Email credentials or recipient not configured.");
                return;
            }

            try
            {
                string subject = $"Thông tin đơn hàng {DisplayName} #{orderCode}";
                string trackingUrl = $"{BaseUrl.TrimEnd('/')}/Order/SearchOrder/{orderCode}";
                string amountHtml = totalAmount > 0 ? $"<p><b>Tổng thanh toán:</b> {totalAmount:N0} đ</p>" : string.Empty;

                string body = $@"<div class='text-center'>
<p>Bạn đã đặt hàng thành công</p>
<p>Cảm ơn bạn vì giữa muôn vàn sự lựa chọn đã chọn <b>{DisplayName}</b></p>
<p><b>Mã đơn hàng:</b> #{orderCode}</p>
{amountHtml}<p>Xem chi tiết đơn đặt hàng của bạn <a href='{trackingUrl}'>tại đây</a></p>
</div>";

                SendEmail(toEmail, subject, body, true);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to send order confirmation email for order #{orderCode}: {ex.Message}");
            }
        }

        public async Task SendOrderConfirmationAsync(string toEmail, string name, string orderCode, decimal totalAmount = 0)
        {
            if (string.IsNullOrWhiteSpace(toEmail) || !IsEmailConfigured)
            {
                Trace.TraceWarning("SendOrderConfirmationAsync skipped: Email credentials or recipient not configured.");
                return;
            }

            try
            {
                string subject = $"Thông tin đơn hàng {DisplayName} #{orderCode}";
                string trackingUrl = $"{BaseUrl.TrimEnd('/')}/Order/SearchOrder/{orderCode}";
                string amountHtml = totalAmount > 0 ? $"<p><b>Tổng thanh toán:</b> {totalAmount:N0} đ</p>" : string.Empty;

                string body = $@"<div class='text-center'>
<p>Bạn đã đặt hàng thành công</p>
<p>Cảm ơn bạn vì giữa muôn vàn sự lựa chọn đã chọn <b>{DisplayName}</b></p>
<p><b>Mã đơn hàng:</b> #{orderCode}</p>
{amountHtml}<p>Xem chi tiết đơn đặt hàng của bạn <a href='{trackingUrl}'>tại đây</a></p>
</div>";

                await SendEmailAsync(toEmail, subject, body, true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to send order confirmation email for order #{orderCode}: {ex.Message}");
            }
        }
    }
}
