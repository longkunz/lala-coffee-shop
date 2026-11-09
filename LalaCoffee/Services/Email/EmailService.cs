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

		private const int SmtpTimeoutMilliseconds = 10000;

		private const int SmtpCancelGraceMilliseconds = 5000;

		public string Host { get; }
		public int Port { get; }
		public bool EnableSsl { get; }
		public string FromAddress { get; }
		public string Username { get; }
		public string FromPassword { get; }
		public string DisplayName { get; }
		public string BaseUrl { get; }

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
			var username = GetSetting("Email_Username", configResolver);
			Username = string.IsNullOrWhiteSpace(username) ? FromAddress : username;

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
				Credentials = new NetworkCredential(Username, FromPassword),
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
					var sendTask = client.SendMailAsync(message);
					var timeoutTask = Task.Delay(SmtpTimeoutMilliseconds);
					var completedTask = await Task.WhenAny(sendTask, timeoutTask).ConfigureAwait(false);
					if (completedTask == timeoutTask)
					{
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
						}

						var settledTask = await Task.WhenAny(
							sendTask, Task.Delay(SmtpCancelGraceMilliseconds)).ConfigureAwait(false);
						if (settledTask == sendTask)
						{
							if (sendTask.Status == TaskStatus.RanToCompletion)
							{
								await sendTask.ConfigureAwait(false);
								return;
							}

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
								<p>Cảm ơn bạn đã lựa chọn <b>{DisplayName}</b></p>
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
								<p>Cảm ơn bạn đã lựa chọn <b>{DisplayName}</b></p>
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
