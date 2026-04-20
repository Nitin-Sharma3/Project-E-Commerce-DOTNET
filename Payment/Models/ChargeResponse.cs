namespace RazorpayApi.Models
{
    public class ChargeResponse
    {
        public bool Success { get; set; }
        public string PaymentId { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}
