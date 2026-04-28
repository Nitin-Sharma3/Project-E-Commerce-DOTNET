using Microsoft.AspNetCore.Mvc;
using OrderAPI.Common;
using OrderAPI.DTOs;
using OrderAPI.Services;
using OrderAPI.Services;
using Serilog;

namespace OrderAPI.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // ── POST api/users/{userId}/orders ────────────────────
    /// <summary>Place a new order from the user's cart.</summary>
    [HttpPost("users/{userId:int}/orders")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PlaceOrder(int userId, [FromBody] PlaceOrderDto dto)
    {
        // All exceptions bubble up to ExceptionMiddleware — no try/catch needed
        var order = await _orderService.PlaceOrderAsync(userId, dto);
        var response = ApiResponse<OrderResponseDto>.Created(order, "Order placed successfully.");
        response.TraceId = HttpContext.TraceIdentifier;

        return CreatedAtAction(nameof(GetOrder),
            new { userId, orderId = order.Id },
            response);
    }

    // ── GET api/users/{userId}/orders ─────────────────────
    /// <summary>List all orders for a user (summary view).</summary>
    [HttpGet("users/{userId:int}/orders")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserOrders(int userId)
    {
        var orders = await _orderService.GetUserOrdersAsync(userId);
        var response = ApiResponse<IEnumerable<OrderSummaryDto>>.Ok(orders);
        response.TraceId = HttpContext.TraceIdentifier;
        return Ok(response);
    }

    // ── GET api/users/{userId}/orders/{orderId} ───────────
    /// <summary>Get full detail of a single order.</summary>
    [HttpGet("users/{userId:int}/orders/{orderId:int}", Name = nameof(GetOrder))]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(int userId, int orderId)
    {
        var order = await _orderService.GetOrderAsync(userId, orderId);
        var response = ApiResponse<OrderResponseDto>.Ok(order);
        response.TraceId = HttpContext.TraceIdentifier;
        return Ok(response);
    }

    // ── PATCH api/orders/{orderId}/status ─────────────────
    /// <summary>Admin: update order status.</summary>
    [HttpPatch("orders/{orderId:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await _orderService.UpdateOrderStatusAsync(orderId, dto);
        var response = ApiResponse<OrderResponseDto>.Ok(order, $"Order status updated to {dto.Status}.");
        response.TraceId = HttpContext.TraceIdentifier;
        return Ok(response);
    }

    // ── DELETE api/users/{userId}/orders/{orderId} ────────
    /// <summary>User: cancel their own order.</summary>
    [HttpDelete("users/{userId:int}/orders/{orderId:int}")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelOrder(int userId, int orderId)
    {
        var order = await _orderService.CancelOrderAsync(userId, orderId);
        var response = ApiResponse<OrderResponseDto>.Ok(order, "Order cancelled successfully.");
        response.TraceId = HttpContext.TraceIdentifier;
        return Ok(response);
    }
}