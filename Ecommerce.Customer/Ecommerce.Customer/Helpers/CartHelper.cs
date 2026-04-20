using Ecommerce.Customer.CartAPI.Models;

namespace Ecommerce.Customer.CartAPI.Helpers
{
    public static class CartHelper
    {
        public static decimal CalculateTotal(List<CartItem> items)
        {
            return items.Sum(i => i.Price * i.Quantity);
        }
    }
}
