 using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
namespace Consumer.MVC.DTOs
{
   
    
        // ── Cart DTOs ──────────────────────────────────────────────
        public class AddToCartDto
        {
            [Required]
            public int ProductId { get; set; }
            [Range(1, 99)]
            public int Quantity { get; set; } = 1;
        }

        public class UpdateCartDto
        {
            [Required]
            public int ProductId { get; set; }
            [Range(1, 99)]
            public int Quantity { get; set; }
        }

        // ── API Response Wrappers ──────────────────────────────────
        public class CartApiResponse
        {
            public List<CartItemDto> Items { get; set; } = new();
            public decimal Subtotal { get; set; }
            public decimal Tax { get; set; }
            public decimal ShippingCost { get; set; }
            public decimal Total { get; set; }
            public decimal Discount { get; set; }
        }

        public class CartItemDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductImage { get; set; }
            public string Category { get; set; }
            public string Brand { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal DiscountedPrice { get; set; }
            public int Quantity { get; set; }
            public bool InStock { get; set; }
            public int MaxQuantity { get; set; }
            public string Sku { get; set; }
        }

        // ── Order DTOs ─────────────────────────────────────────────
        public class PlaceOrderDto
        {
            [Required]
            public int AddressId { get; set; }
            [Required]
            public string PaymentMethod { get; set; }
            public string Notes { get; set; }
            public string CouponCode { get; set; }
        }

        public class UpdateOrderStatusDto
        {
            [Required]
            public string Status { get; set; }
            public string Note { get; set; }
        }

        public class OrderApiResponse
        {
            public int Id { get; set; }
            public string OrderNumber { get; set; }
            public string Status { get; set; }
            public string PaymentMethod { get; set; }
            public string PaymentStatus { get; set; }
            public System.DateTime PlacedAt { get; set; }
            public List<OrderItemDto> Items { get; set; } = new();
            public InlineAddressDto ShippingAddress { get; set; }
            public decimal Subtotal { get; set; }
            public decimal Tax { get; set; }
            public decimal ShippingCost { get; set; }
            public decimal Discount { get; set; }
            public decimal Total { get; set; }
            public string TrackingNumber { get; set; }
            public List<OrderTimelineDto> Timeline { get; set; } = new();
        }

        public class OrderItemDto
        {
            public string ProductName { get; set; }
            public string ProductImage { get; set; }
            public string Category { get; set; }
            public string Sku { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal LineTotal { get; set; }
        }

        public class InlineAddressDto
        {
            public string FullName { get; set; }
            public string Line1 { get; set; }
            public string Line2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string PostalCode { get; set; }
            public string Country { get; set; }
            public string Phone { get; set; }
        }

        public class OrderTimelineDto
        {
            public string Status { get; set; }
            public string Description { get; set; }
            public System.DateTime Timestamp { get; set; }
            public bool IsCompleted { get; set; }
            public bool IsCurrent { get; set; }
        }

        public enum OrderStatus
        {
            Pending, Confirmed, Processing, Shipped, Delivered, Cancelled, Refunded
        }

        public enum PaymentMethod
        {
            CreditCard, DebitCard, UPI, NetBanking, CashOnDelivery, Wallet
        }

        public enum AddressType
        {
            Home, Work, Other
        }
    
}
