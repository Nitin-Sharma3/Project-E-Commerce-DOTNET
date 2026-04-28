namespace UserDashboardMVC.ViewModels
{
 
    public class OrderItemViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class ShippingAddressViewModel
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

    public class OrderViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public ShippingAddressViewModel ShippingAddress { get; set; } = new();
        public List<OrderItemViewModel> Items { get; set; } = new();

        // Computed helpers for UI
        public int TotalItems => Items.Sum(i => i.Quantity);
        public string StatusIcon => Status switch
        {
            "Pending" => "⏳",
            "Confirmed" => "✅",
            "Shipped" => "🚚",
            "Delivered" => "📦",
            "Cancelled" => "❌",
            _ => "•"
        };
        public bool CanCancel => Status is "Pending" or "Confirmed";
    }

    public class OrderSummaryViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
    }

    public class OrderListPageViewModel
    {
        public int UserId { get; set; } = 1;
        public List<OrderSummaryViewModel> Orders { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        // Filter state
        public string ActiveFilter { get; set; } = "All";
        public List<OrderSummaryViewModel> FilteredOrders =>
            ActiveFilter == "All"
                ? Orders
                : Orders.Where(o => o.Status == ActiveFilter).ToList();
    }

    public class OrderDetailPageViewModel
    {
        public OrderViewModel Order { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }

}
