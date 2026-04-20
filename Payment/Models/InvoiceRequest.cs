namespace RazorpayApi.Models
{
    public class InvoiceRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = "Razorpay";
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string Currency { get; set; } = "INR";
        public SellerDetails? SellerDetails { get; set; }
        public CustomerDetails? CustomerDetails { get; set; }
        public string? TermsAndConditions { get; set; }
    }
}
