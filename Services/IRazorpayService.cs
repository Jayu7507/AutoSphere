using System.Collections.Generic;

namespace AutoSphere.Services
{
    public interface IRazorpayService
    {
        string CreateOrder(decimal amount, string receipt, string? notes = null);
        bool VerifyPaymentSignature(string orderId, string paymentId, string signature);
    }
}
