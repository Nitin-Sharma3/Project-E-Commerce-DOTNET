using System.ComponentModel.DataAnnotations;
namespace OrderAPI.DTOs
{
    

    // ── Inbound ──────────────────────────────────────────────

    //public class ShippingAddressDto
    //{
    //    [Required] public string FullName { get; set; } = string.Empty;
    //    [Required] public string Phone { get; set; } = string.Empty;
    //    [Required] public string AddressLine1 { get; set; } = string.Empty;
    //    public string? AddressLine2 { get; set; }
    //    [Required] public string City { get; set; } = string.Empty;
    //    [Required] public string State { get; set; } = string.Empty;
    //    [Required] public string PostalCode { get; set; } = string.Empty;
    //    [Required] public string Country { get; set; } = string.Empty;
    //}

    public class PlaceOrderDto
    {
        
        public int AddressId { get; set; }  
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty; 
    }

    // ── Outbound ─────────────────────────────────────────────

    public class OrderItemResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
    public class SnapshotAddressDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public SnapshotAddressDto ShippingAddress { get; set; } = null!;
        public List<OrderItemResponseDto> Items { get; set; } = new();
    }

    public class OrderSummaryDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
    }
}
