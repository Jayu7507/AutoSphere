using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using System;
using System.Collections.Generic;

namespace AutoSphere.Services
{
    public class RazorpayService : IRazorpayService
    {
        private readonly IConfiguration _config;
        private readonly string _keyId;
        private readonly string _keySecret;

        public RazorpayService(IConfiguration config)
        {
            _config = config;
            _keyId = _config["Razorpay:KeyId"] ?? "";
            _keySecret = _config["Razorpay:KeySecret"] ?? "";
        }

        public string CreateOrder(decimal amount, string receipt, string? notes = null)
        {
            if (string.IsNullOrEmpty(_keyId) || string.IsNullOrEmpty(_keySecret))
            {
                throw new InvalidOperationException("Razorpay API keys are missing in configuration.");
            }

            try
            {
                // Amount must be in paise
                int amountInPaise = Convert.ToInt32(Math.Round(amount * 100));

                var client = new RazorpayClient(_keyId, _keySecret);

                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", amountInPaise);
                options.Add("currency", "INR");
                options.Add("receipt", receipt);

                if (!string.IsNullOrEmpty(notes))
                {
                    Dictionary<string, string> notesList = new Dictionary<string, string>()
                    {
                        { "note", notes }
                    };
                    options.Add("notes", notesList);
                }

                Order order = client.Order.Create(options);
                
                if (order == null || !order.Attributes.ContainsKey("id"))
                {
                    throw new Exception("Razorpay API returned an empty or invalid order response.");
                }

                return order!["id"]?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Razorpay Error] Failed to create order: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Razorpay Inner Error] {ex.InnerException.Message}");
                }
                throw; // Rethrow to let the controller handle it
            }
        }

        public bool VerifyPaymentSignature(string orderId, string paymentId, string signature)
        {
            try
            {
                string payload = orderId + "|" + paymentId;
                using (var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(_keySecret)))
                {
                    byte[] hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
                    string generatedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    return generatedSignature == signature;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
