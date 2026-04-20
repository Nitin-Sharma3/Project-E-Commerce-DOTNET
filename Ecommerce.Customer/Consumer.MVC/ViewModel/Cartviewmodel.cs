 using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
namespace Consumer.MVC.ViewModel
{
   
  
        public class CartViewModel
        {
            public List<CartItemViewModel> Items { get; set; } = new();
            public decimal Subtotal { get; set; }
            public decimal Tax { get; set; }
            public decimal Shipping { get; set; }
            public decimal Total { get; set; }
            public int ItemCount { get; set; }
            public string CouponCode { get; set; }
            public decimal Discount { get; set; }
        }

        public class CartItemViewModel
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductImage { get; set; }
            public string Category { get; set; }
            public string Brand { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal DiscountedPrice { get; set; }
            public int Quantity { get; set; }
            public decimal LineTotal => DiscountedPrice * Quantity;
            public bool InStock { get; set; }
            public int MaxQuantity { get; set; }
            public string Sku { get; set; }
        }

        public class UpdateCartViewModel
        {
            [Required]
            public int ProductId { get; set; }
            [Range(1, 99)]
            public int Quantity { get; set; }
        }
    }

