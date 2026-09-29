using System.Threading.Tasks;

namespace AutoSphere.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendOTPEmailAsync(string toEmail, string otp);
    }
}
