using OrderAPI.DTOs;
using OrderAPI.HttpClients;
using OrderAPI.Models;
using OrderAPI.Repositories;
using OrderAPI.Services;
namespace OrderAPI.Services
{
    

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartClient _cartClient;
        private readonly IAddressClient _addressClient;
        private readonly IDeliveryClient _deliveryClient;

        public OrderService(
            IOrderRepository orderRepository,
            ICartClient cartClient,
            IAddressClient addressClient,
            IDeliveryClient deliveryClient)
        {
            _orderRepository = orderRepository;
            _cartClient = cartClient;
            _addressClient = addressClient;
            _deliveryClient = deliveryClient;
        }

        // ── Mapping helpers ───────────────────────────────────

        private static OrderResponseDto MapToResponse(Order order) => new()
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            ShippingAddress = new SnapshotAddressDto
            {
                FullName = order.ShippingAddress.FullName,
                Phone = order.ShippingAddress.Phone,
                AddressLine1 = order.ShippingAddress.AddressLine1,
                AddressLine2 = order.ShippingAddress.AddressLine2,
                City = order.ShippingAddress.City,
                State = order.ShippingAddress.State,
                PostalCode = order.ShippingAddress.PostalCode,
                Country = order.ShippingAddress.Country
            },
            Items = order.OrderItems.Select(oi => new OrderItemResponseDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Subtotal = oi.Subtotal
            }).ToList()
        };

        private static OrderSummaryDto MapToSummary(Order order) => new()
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            TotalItems = order.OrderItems.Sum(oi => oi.Quantity)
        };

        // ── Business Logic ────────────────────────────────────

        public async Task<OrderResponseDto> PlaceOrderAsync(int userId, PlaceOrderDto dto)
        {
            // 1. Validate address belongs to this user via Address API
            var address = await _addressClient.GetAddressAsync(userId, dto.AddressId)
                          ?? throw new KeyNotFoundException(
                                 $"Address {dto.AddressId} not found or does not belong to this user.");

            // 2. Fetch cart from Cart API
            var cart = await _cartClient.GetCartAsync(userId);

            if (cart is null || cart.Items.Count == 0)
                throw new InvalidOperationException("Cannot place an order with an empty cart.");

            // 3. Snapshot cart items (prices locked at order time)
            var orderItems = cart.Items.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                ProductName = $"Product #{ci.ProductId}",
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice
            }).ToList();

            // 4. Snapshot address fields into order (not a FK — historical record)
            var order = new Order
            {
                UserId = userId,
                TotalAmount = cart.TotalAmount,
                ShippingAddress = new ShippingAddress
                {
                    FullName = address.FullName,
                    Phone = address.Phone,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    Country = address.Country
                },
                OrderItems = orderItems
            };

            var created = await _orderRepository.CreateAsync(order);

            // 5. Clear cart after successful order
            await _cartClient.ClearCartAsync(userId);
            // ── Notify DeliveryAPI (fire and forget) ──────────────────────────
            _ = Task.Run(() => _deliveryClient.CreateDeliveryAsync(created.Id, userId));
            //await _deliveryClient.CreateDeliveryAsync(created.Id);

            return MapToResponse(created);
        }

        public async Task<OrderResponseDto> GetOrderAsync(int userId, int orderId)
        {
            var order = await _orderRepository.GetByIdAndUserAsync(orderId, userId)
                        ?? throw new KeyNotFoundException($"Order {orderId} not found.");

            return MapToResponse(order);
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetUserOrdersAsync(int userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return orders.Select(MapToSummary);
        }

        public async Task<OrderResponseDto> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(orderId)
                        ?? throw new KeyNotFoundException($"Order {orderId} not found.");

            if (!Enum.TryParse<OrderStatus>(dto.Status, ignoreCase: true, out var newStatus))
                throw new ArgumentException($"Invalid status '{dto.Status}'. Valid values: Confirmed, Shipped, Delivered, Cancelled.");

            if (order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Cannot update a cancelled order.");

            if (order.Status == OrderStatus.Delivered)
                throw new InvalidOperationException("Cannot update a delivered order.");

            if (newStatus == OrderStatus.Pending)
                throw new InvalidOperationException("Cannot revert an order back to Pending.");

            order.Status = newStatus;
            await _orderRepository.UpdateAsync(order);

            return MapToResponse(order);
        }

        public async Task<OrderResponseDto> CancelOrderAsync(int userId, int orderId)
        {
            var order = await _orderRepository.GetByIdAndUserAsync(orderId, userId)
                        ?? throw new KeyNotFoundException($"Order {orderId} not found.");

            if (order.Status is OrderStatus.Shipped or OrderStatus.Delivered)
                throw new InvalidOperationException($"Cannot cancel an order that is already {order.Status}.");

            if (order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Order is already cancelled.");

            order.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateAsync(order);

            return MapToResponse(order);
        }
    }

}
