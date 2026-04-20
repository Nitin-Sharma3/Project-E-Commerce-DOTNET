namespace RazorpayApi.Models
{
    public class RefundResponse
    {
        public bool Success { get; set; }
        public string? RefundId { get; set; }
        public string? PaymentId { get; set; }
        public int? Amount { get; set; }
        public string? Status { get; set; }
        public string? Speed { get; set; }
        public string? Receipt { get; set; }
        public string? Notes { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}