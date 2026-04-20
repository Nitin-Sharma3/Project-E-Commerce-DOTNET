namespace RazorpayApi.Models
{
    public class PaymentDetailsResponse
    {
        public string OrderId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public List<OrderItem> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public SellerDetails? SellerDetails { get; set; }
        public CustomerDetails? CustomerDetails { get; set; }
    }
}
