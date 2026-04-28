using UserDashboardMVC.Models;

namespace UserDashboardMVC.ViewModels
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Variant { get; set; } = string.Empty;       // e.g. "Space Grey / 256GB"
        public string ImageUrl { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => UnitPrice * Quantity;
    }

    public class CartPageViewModel
    {
        public int UserId { get; set; } = 1;
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal SubTotal => Items.Sum(i => i.Subtotal);
        public decimal Shipping => 0;                             // "Calculated at next step"
        public decimal Tax => 0;
        public decimal Total => SubTotal + Shipping + Tax;
        public int TotalItems => Items.Sum(i => i.Quantity);

        // "Complete the Look" — other products from API
        public List<ProductDto> SuggestedProducts { get; set; } = new();
    }
}
