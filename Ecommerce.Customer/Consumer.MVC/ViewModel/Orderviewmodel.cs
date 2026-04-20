
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
namespace Consumer.MVC.ViewModel
{

        public class OrderListViewModel
        {
            public List<OrderSummaryViewModel> Orders { get; set; } = new();
            public int TotalOrders { get; set; }
            public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
            public string StatusFilter { get; set; }
        }

        public class OrderSummaryViewModel
        {
            public int Id { get; set; }
            public string OrderNumber { get; set; }
            public DateTime PlacedAt { get; set; }
            public string Status { get; set; }
            public string StatusColor { get; set; }
            public decimal Total { get; set; }
            public int ItemCount { get; set; }
            public string ThumbnailUrl { get; set; }
            public string FirstProductName { get; set; }
        }

        public class OrderDetailViewModel
        {
            public int Id { get; set; }
            public string OrderNumber { get; set; }
            public DateTime PlacedAt { get; set; }
            public string Status { get; set; }
            public string PaymentMethod { get; set; }
            public string PaymentStatus { get; set; }
            public List<OrderItemViewModel> Items { get; set; } = new();
            public AddressViewModel ShippingAddress { get; set; }
            public decimal Subtotal { get; set; }
            public decimal Tax { get; set; }
            public decimal ShippingCost { get; set; }
            public decimal Discount { get; set; }
            public decimal Total { get; set; }
            public List<OrderTimelineEvent> Timeline { get; set; } = new();
            public bool CanCancel { get; set; }
            public string TrackingNumber { get; set; }
        }

        public class OrderItemViewModel
        {
            public string ProductName { get; set; }
            public string ProductImage { get; set; }
            public string Category { get; set; }
            public string Sku { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal LineTotal { get; set; }
        }

       
        public class OrderTimelineEvent
        {
            public string Status { get; set; }
            public string Description { get; set; }
            public DateTime Timestamp { get; set; }
            public bool IsCompleted { get; set; }
            public bool IsCurrent { get; set; }
            public string Icon { get; set; }
        }

        public class PlaceOrderViewModel
        {
            [Required]
            public int AddressId { get; set; }
            [Required]
            public string PaymentMethod { get; set; }
            public List<AddressViewModel> SavedAddresses { get; set; } = new();
            public CartViewModel Cart { get; set; }
            public string Notes { get; set; }
        }
    }

