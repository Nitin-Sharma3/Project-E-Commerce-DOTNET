using AutoMapper;
using FluentValidation;
using OrderAPI.Common;

using OrderAPI.DTOs;
using OrderAPI.HttpClients;
using OrderAPI.Models;
using OrderAPI.Repositories;

using Serilog;
using ValidationException = OrderAPI.Common.ValidationException;

namespace OrderAPI.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartClient _cartClient;
    private readonly IAddressClient _addressClient;
    private readonly IMapper _mapper;
    private readonly IValidator<PlaceOrderDto> _placeValidator;
    private readonly IValidator<UpdateOrderStatusDto> _statusValidator;

    public OrderService(
        IOrderRepository orderRepository,
        ICartClient cartClient,
        IAddressClient addressClient,
        IMapper mapper,
        IValidator<PlaceOrderDto> placeValidator,
        IValidator<UpdateOrderStatusDto> statusValidator)
    {
        _orderRepository = orderRepository;
        _cartClient = cartClient;
        _addressClient = addressClient;
        _mapper = mapper;
        _placeValidator = placeValidator;
        _statusValidator = statusValidator;
    }

    // ─────────────────────────────────────────────────────
    // Place Order
    // ─────────────────────────────────────────────────────
    public async Task<OrderResponseDto> PlaceOrderAsync(int userId, PlaceOrderDto dto)
    {
        // 1. FluentValidation
        var validation = await _placeValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        Log.Information("Placing order for User {UserId} with AddressId {AddressId}", userId, dto.AddressId);

        // 2. Validate address ownership via AddressAPI
        var address = await _addressClient.GetAddressAsync(userId, dto.AddressId)
                      ?? throw new NotFoundException($"Address {dto.AddressId} not found or does not belong to user {userId}.");

        // 3. Fetch cart from CartAPI
        var cart = await _cartClient.GetCartAsync(userId);
        if (cart is null || cart.Items.Count == 0)
            throw new BadRequestException("Cannot place an order with an empty cart.");

        // 4. Business rule — calculated field: recompute total server-side (never trust client total)
        var computedTotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);

        // 5. Snapshot cart items via AutoMapper
        var orderItems = _mapper.Map<List<OrderItem>>(cart.Items);

        // 6. Snapshot address via AutoMapper
        var shippingAddress = _mapper.Map<ShippingAddress>(address);

        // 7. Build order entity
        var order = new Order
        {
            UserId = userId,
            TotalAmount = computedTotal,        // server-calculated
            ShippingAddress = shippingAddress,
            OrderItems = orderItems
        };

        var created = await _orderRepository.CreateAsync(order);

        Log.Information("Order {OrderId} created for User {UserId}. Total: {Total}",
            created.Id, userId, computedTotal);

        // 8. Clear cart post-order
        await _cartClient.ClearCartAsync(userId);

        return _mapper.Map<OrderResponseDto>(created);
    }

    // ─────────────────────────────────────────────────────
    // Get Single Order
    // ─────────────────────────────────────────────────────
    public async Task<OrderResponseDto> GetOrderAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetByIdAndUserAsync(orderId, userId)
                    ?? throw new NotFoundException("Order", orderId);

        return _mapper.Map<OrderResponseDto>(order);
    }

    // ─────────────────────────────────────────────────────
    // Get All Orders for User
    // ─────────────────────────────────────────────────────
    public async Task<IEnumerable<OrderSummaryDto>> GetUserOrdersAsync(int userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);

        // Calculated field: TotalItems summed per order in AutoMapper profile
        return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
    }

    // ─────────────────────────────────────────────────────
    // Update Order Status (Admin)
    // ─────────────────────────────────────────────────────
    public async Task<OrderResponseDto> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto)
    {
        // FluentValidation
        var validation = await _statusValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var order = await _orderRepository.GetByIdAsync(orderId)
                    ?? throw new NotFoundException("Order", orderId);

        if (!Enum.TryParse<OrderStatus>(dto.Status, ignoreCase: true, out var newStatus))
            throw new BadRequestException(
                $"Invalid status '{dto.Status}'.",
                new List<string> { "Valid values: Confirmed, Shipped, Delivered, Cancelled." });

        // ── Business rules: status transition guard ───────────
        if (order.Status == OrderStatus.Cancelled)
            throw new ConflictException("Cannot update a cancelled order.");

        if (order.Status == OrderStatus.Delivered)
            throw new ConflictException("Cannot update a delivered order.");

        if (newStatus == OrderStatus.Pending)
            throw new ConflictException("Cannot revert an order back to Pending.");

        var previous = order.Status;
        order.Status = newStatus;
        await _orderRepository.UpdateAsync(order);

        Log.Information("Order {OrderId} status changed: {From} → {To}", orderId, previous, newStatus);

        return _mapper.Map<OrderResponseDto>(order);
    }

    // ─────────────────────────────────────────────────────
    // Cancel Order (User)
    // ─────────────────────────────────────────────────────
    public async Task<OrderResponseDto> CancelOrderAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetByIdAndUserAsync(orderId, userId)
                    ?? throw new NotFoundException("Order", orderId);

        // ── Business rules ────────────────────────────────────
        if (order.Status == OrderStatus.Cancelled)
            throw new ConflictException("This order is already cancelled.");

        if (order.Status is OrderStatus.Shipped or OrderStatus.Delivered)
            throw new ConflictException($"Cannot cancel an order that is already {order.Status}.");

        order.Status = OrderStatus.Cancelled;
        await _orderRepository.UpdateAsync(order);

        Log.Information("Order {OrderId} cancelled by User {UserId}", orderId, userId);

        return _mapper.Map<OrderResponseDto>(order);
    }
}
