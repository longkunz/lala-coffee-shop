using System.Net.Mail;
using System.Threading.Tasks;

namespace LalaCoffee.Services.Email
{
    public interface IEmailService
    {
        void SendEmail(string toEmail, string subject, string body, bool isHtml = true);
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
        void SendOrderConfirmation(string toEmail, string name, string orderCode, decimal totalAmount = 0);
        Task SendOrderConfirmationAsync(string toEmail, string name, string orderCode, decimal totalAmount = 0);
    }
}
