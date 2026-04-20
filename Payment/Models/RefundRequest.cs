namespace RazorpayApi.Models
{
    public class RefundRequest
    {
        public string PaymentId { get; set; } = string.Empty;
        public int? Amount { get; set; }
        public string? Notes { get; set; }
        public string? IdempotencyKey { get; set; }
        public string? Receipt { get; set; }
        public string RefundSpeed { get; set; } = "optimized";
    }
}