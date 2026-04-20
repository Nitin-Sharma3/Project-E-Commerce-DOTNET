using Ecommerce.Customer.OrderAPI.Data;
using Ecommerce.Customer.OrderAPI.DTOs;
using Ecommerce.Customer.OrderAPI.Models;
using Ecommerece.Customer.Address.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Customer.OrderAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderDbContext _db;
        private readonly ICartServiceClient _cartClient;       // HTTP client → Cart API
        private readonly IAddressServiceClient _addressClient; // HTTP client → Address API

        public OrderService(
            OrderDbContext db,
            ICartServiceClient cartClient,
            IAddressServiceClient addressClient)
        {
            _db = db;
            _cartClient = cartClient;
            _addressClient = addressClient;
        }

        public async Task<OrderResponseDto> PlaceOrder(string userId, PlaceOrderDto dto)
        {
            // 1. Fetch cart from Cart API
            var cart = await _cartClient.GetCart(userId);
            if (cart == null || !cart.Items.Any())
                throw new InvalidOperationException("Cart is empty.");

            // 2. Resolve delivery address
            OrderAddress deliveryAddress;
            if (dto.AddressId.HasValue)
            {
                var addr = await _addressClient.GetAddress(userId, dto.AddressId.Value);
                if (addr == null) throw new InvalidOperationException("Address not found.");
                deliveryAddress = MapAddress(addr);
            }
            else if (dto.InlineAddress != null)
            {
                deliveryAddress = MapInlineAddress(dto.InlineAddress);
            }
            else
            {
                throw new InvalidOperationException("Supply either AddressId or InlineAddress.");
            }

            // 3. Calculate financials
            var subTotal = cart.TotalAmount;
            var deliveryCharge = subTotal >= 499 ? 0m : 49m;   // free above ₹499
            var discount = 0m;                                   // apply coupon logic here
            var total = subTotal + deliveryCharge - discount;

            // 4. Build order entity
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                DeliveryAddress = deliveryAddress,
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Name = i.Name,
                    Image = i.Image,
                    UnitPrice = i.Price,
                    Quantity = i.Quantity,
                    SubTotal = i.SubTotal
                }).ToList(),
                SubTotal = subTotal,
                DeliveryCharge = deliveryCharge,
                Discount = discount,
                TotalAmount = total,
                PaymentMethod = dto.PaymentMethod,
                Notes = dto.Notes,
                PlacedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // 5. Clear cart after successful order (fire-and-forget is fine here)
            await _cartClient.ClearCart(userId);

            return MapToResponse(order);
        }

        public async Task<List<OrderListDto>> GetOrders(string userId)
        {
            return await _db.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.PlacedAt)
                .Select(o => new OrderListDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.Items.Count,
                    Status = o.Status.ToString(),
                    PaymentStatus = o.PaymentStatus.ToString(),
                    PlacedAt = o.PlacedAt
                })
                .ToListAsync();
        }

        public async Task<OrderResponseDto?> GetOrderById(string userId, int orderId)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.DeliveryAddress)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            return order == null ? null : MapToResponse(order);
        }

        public async Task<OrderResponseDto?> GetOrderByNumber(string userId, string orderNumber)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.DeliveryAddress)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.UserId == userId);

            return order == null ? null : MapToResponse(order);
        }

        public async Task<OrderResponseDto?> UpdateStatus(int orderId, UpdateOrderStatusDto dto)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.DeliveryAddress)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            order.Status = dto.Status;
            order.UpdatedAt = DateTime.UtcNow;

            switch (dto.Status)
            {
                case OrderStatus.Confirmed: order.ConfirmedAt = DateTime.UtcNow; break;
                case OrderStatus.Shipped: order.ShippedAt = DateTime.UtcNow; break;
                case OrderStatus.Delivered:
                    order.DeliveredAt = DateTime.UtcNow;
                    order.PaymentStatus = PaymentStatus.Paid;   // auto-mark COD as paid on delivery
                    break;
                case OrderStatus.Cancelled:
                    order.CancelledAt = DateTime.UtcNow;
                    order.CancellationReason = dto.CancellationReason;
                    break;
            }

            await _db.SaveChangesAsync();
            return MapToResponse(order);
        }

        public async Task<bool> CancelOrder(string userId, int orderId, string? reason)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null) return false;

            // Only allow cancellation before shipment
            if (order.Status >= OrderStatus.Shipped)
                throw new InvalidOperationException("Cannot cancel a shipped order.");

            order.Status = OrderStatus.Cancelled;
            order.CancellationReason = reason;
            order.CancelledAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string GenerateOrderNumber()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var suffix = Guid.NewGuid().ToString("N")[..6].ToUpper();
            return $"ORD-{date}-{suffix}";
        }

        private static OrderAddress MapAddress(AddressResponseDto addr) => new()
        {
            FullName = addr.FullName,
            PhoneNumber = addr.PhoneNumber,
            AddressLine1 = addr.AddressLine1,
            AddressLine2 = addr.AddressLine2,
            City = addr.City,
            State = addr.State,
            PostalCode = addr.PostalCode,
            Country = addr.Country,
            Label = addr.Label
        };

        private static OrderAddress MapInlineAddress(InlineAddressDto addr) => new()
        {
            FullName = addr.FullName,
            PhoneNumber = addr.PhoneNumber,
            AddressLine1 = addr.AddressLine1,
            AddressLine2 = addr.AddressLine2,
            City = addr.City,
            State = addr.State,
            PostalCode = addr.PostalCode,
            Country = addr.Country
        };

        private static OrderResponseDto MapToResponse(Order o) => new()
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            UserId = o.UserId,
            DeliveryAddress = new OrderAddressDto
            {
                FullName = o.DeliveryAddress.FullName,
                PhoneNumber = o.DeliveryAddress.PhoneNumber,
                AddressLine1 = o.DeliveryAddress.AddressLine1,
                AddressLine2 = o.DeliveryAddress.AddressLine2,
                City = o.DeliveryAddress.City,
                State = o.DeliveryAddress.State,
                PostalCode = o.DeliveryAddress.PostalCode,
                Country = o.DeliveryAddress.Country,
                Label = o.DeliveryAddress.Label
            },
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                Name = i.Name,
                Image = i.Image,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                SubTotal = i.SubTotal
            }).ToList(),
            SubTotal = o.SubTotal,
            DeliveryCharge = o.DeliveryCharge,
            Discount = o.Discount,
            TotalAmount = o.TotalAmount,
            Status = o.Status.ToString(),
            PaymentStatus = o.PaymentStatus.ToString(),
            PaymentMethod = o.PaymentMethod.ToString(),
            Notes = o.Notes,
            CancellationReason = o.CancellationReason,
            PlacedAt = o.PlacedAt,
            ShippedAt = o.ShippedAt,
            DeliveredAt = o.DeliveredAt
        };
    }
}