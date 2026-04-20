using Ecommerce.Customer.OrderAPI.Models;

namespace Ecommerce.Customer.OrderAPI.DTOs
{
    public class UpdateOrderStatusDto
    {
        public OrderStatus Status { get; set; }
        public string? CancellationReason { get; set; }   // required if Status = Cancelled
    }
}