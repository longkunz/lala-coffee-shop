using System.Collections.Specialized;
using System.Threading.Tasks;
using LalaCoffee.Models;
using LalaCoffee.Services.Payment.Momo;

namespace LalaCoffee.Services.Payment
{
    public interface IPaymentService
    {
        // MoMo
        Task<string> CreateMomoPaymentUrlAsync(Order order, string returnUrl = null, string ipnUrl = null, string extraData = "");
        string CreateMomoPaymentUrl(Order order, string returnUrl = null, string ipnUrl = null, string extraData = "");
        bool ValidateMomoResponse(MomoResult result);
        bool ValidateMomoResponse(NameValueCollection parameters);

        // VNPay
        string CreateVnPayPaymentUrl(Order order, string ipAddress);
        bool ValidateVnPayResponse(NameValueCollection queryString);
    }
}
