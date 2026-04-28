namespace UserDashboardMVC.Models
{
    // ─── View Model ───────────────────────────────────────────────────────────────
    public class HomeIndexViewModel
    {
        public int UserId { get; set; }
        public List<ProductDto> Products { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public int CartItemCount { get; set; }
        public bool ProductApiError { get; set; }
    }

    // ─── DTOs (mirror your API response shapes) ───────────────────────────────────
    public class ProductDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl1 { get; set; }
        public string? ImageUrl2 { get; set; }
        public string? ImageUrl3 { get; set; }
    }

    public class CartDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<CartItem> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
    }

    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class AddressDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public bool IsPrimary { get; set; }
    }

    // ─── Request bodies (incoming JSON from the view) ─────────────────────────────
    public class AddCartItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class UpdateCartItemRequest
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveCartItemRequest
    {
        public int CartItemId { get; set; }
    }

    public class CreateAddressRequest
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class PlaceOrderRequest
    {
        public int AddressId { get; set; }
    }
}