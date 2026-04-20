using RazorpayApi.Models;

namespace RazorpayApi.Services
{
    public interface IInvoiceService
    {
        InvoiceResponse GenerateInvoicePdf(InvoiceRequest request);
        string GenerateInvoiceFileName(string orderId, string paymentId);
    }
}