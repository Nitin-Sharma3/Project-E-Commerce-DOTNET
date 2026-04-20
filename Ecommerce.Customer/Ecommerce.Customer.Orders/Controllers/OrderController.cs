using Ecommerce.Customer.OrderAPI.DTOs;
using Ecommerce.Customer.OrderAPI.Models;
using Ecommerce.Customer.OrderAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Customer.OrderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // TEMP: replace with real auth (e.g. JWT claim) later
        private string GetUserId() => "user1";

        // POST api/order  — place a new order from cart
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(PlaceOrderDto dto)
        {
            try
            {
                var result = await _orderService.PlaceOrder(GetUserId(), dto);
                return CreatedAtAction(nameof(GetOrderById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET api/order  — list all orders for user
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var result = await _orderService.GetOrders(GetUserId());
            return Ok(result);
        }

        // GET api/order/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var result = await _orderService.GetOrderById(GetUserId(), id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // GET api/order/number/{orderNumber}
        [HttpGet("number/{orderNumber}")]
        public async Task<IActionResult> GetOrderByNumber(string orderNumber)
        {
            var result = await _orderService.GetOrderByNumber(GetUserId(), orderNumber);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // PUT api/order/{id}/status  — admin/delivery use
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusDto dto)
        {
            var result = await _orderService.UpdateStatus(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // DELETE api/order/{id}/cancel  — user-initiated cancel
        [HttpDelete("{id:int}/cancel")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] string? reason)
        {
            try
            {
                var success = await _orderService.CancelOrder(GetUserId(), id, reason);
                if (!success) return NotFound();
                return Ok(new { message = "Order cancelled successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}