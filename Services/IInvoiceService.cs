using AutoSphere.Models;
using System.Threading.Tasks;

namespace AutoSphere.Services
{
    public interface IInvoiceService
    {
        Task<byte[]> GenerateInvoicePdfAsync(Booking booking);
    }
}
