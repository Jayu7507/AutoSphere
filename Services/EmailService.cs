using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace AutoSphere.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailSettings = _config.GetSection("EmailSettings");
            var senderEmail = emailSettings["SenderEmail"] ?? "";
            var senderPassword = emailSettings["SenderPassword"] ?? "";
            var smtpHost = emailSettings["SmtpHost"] ?? "";
            var port = int.TryParse(emailSettings["Port"], out var p) ? p : 587;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("AutoSphere", senderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(smtpHost, port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, senderPassword);
                await client.SendAsync(message);
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                }
            }
        }

        public async Task SendOTPEmailAsync(string toEmail, string otp)
        {
            string body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #0B0F19; color: #FFF;'>
                <h2 style='text-align: center; color: #0F52BA;'>AutoSphere</h2>
                <h3 style='color: #FF6B00;'>Your Verification Code</h3>
                <p style='font-size: 16px; color: #C0C0C0;'>Dear user,</p>
                <p style='font-size: 16px; color: #C0C0C0;'>Use the following OTP to complete your registration or login. This code is valid for 5 minutes.</p>
                <div style='text-align: center; margin: 20px 0;'>
                    <span style='font-size: 24px; font-weight: bold; padding: 10px 20px; background-color: #0F52BA; color: #FFF; border-radius: 5px; letter-spacing: 5px;'>{otp}</span>
                </div>
                <p style='font-size: 14px; color: #FF6B00;'>If you didn't request this, please ignore this email.</p>
                <hr style='border: 0; border-top: 1px solid #333;' />
                <p style='font-size: 12px; text-align: center; color: #888;'>&copy; {System.DateTime.Now.Year} AutoSphere. All rights reserved.</p>
            </div>";

            await SendEmailAsync(toEmail, "AutoSphere - OTP Verification", body);
        }
    }
}
