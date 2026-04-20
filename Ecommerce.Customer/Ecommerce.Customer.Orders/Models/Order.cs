namespace Ecommerce.Customer.OrderAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }       // e.g. "ORD-20240416-0001"
        public string UserId { get; set; }
        public OrderAddress DeliveryAddress { get; set; }

        public List<OrderItem> Items { get; set; } = new();

        public decimal SubTotal { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public PaymentMethod PaymentMethod { get; set; }

        public string? Notes { get; set; }             // buyer notes
        public string? CancellationReason { get; set; }

        public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }

    public enum OrderStatus
    {
        Pending = 1,
        Confirmed = 2,
        Processing = 3,
        Shipped = 4,
        OutForDelivery = 5,
        Delivered = 6,
        Cancelled = 7,
        ReturnRequested = 8,
        Returned = 9
    }

    public enum PaymentStatus
    {
        Unpaid = 1,
        Paid = 2,
        Refunded = 3,
        Failed = 4
    }

    public enum PaymentMethod
    {
        CashOnDelivery = 1,
        UPI = 2,
        Card = 3,
        NetBanking = 4,
        Wallet = 5
    }
}