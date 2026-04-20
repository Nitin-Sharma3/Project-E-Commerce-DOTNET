namespace Ecommerce.Customer.OrderAPI.DTOs
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string UserId { get; set; }

        public OrderAddressDto DeliveryAddress { get; set; }
        public List<OrderItemDto> Items { get; set; }

        public decimal SubTotal { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }

        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }

        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }

        public DateTime PlacedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }

    public class OrderItemDto
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
    }

    public class OrderAddressDto
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string? Label { get; set; }
    }
}