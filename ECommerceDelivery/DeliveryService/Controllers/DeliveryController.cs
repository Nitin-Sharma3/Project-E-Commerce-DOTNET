using DeliveryService.DTOs;
using DeliveryService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.Controller
{
    [ApiController, Route("api/delivery")]
    public class DeliveryController(
     IDeliveryService svc,
     ILogger<DeliveryController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(new ApiResponse<object>(true, "Success", await svc.GetAllAsync()));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var d = await svc.GetByIdAsync(id);
            return d == null
                ? NotFound(new ApiResponse<object>(false, $"Delivery {id} not found", null))
                : Ok(new ApiResponse<object>(true, "Success", d));
        }

        [HttpGet("order/{orderId:int}")]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            var d = await svc.GetByOrderIdAsync(orderId);
            return d == null
                ? NotFound(new ApiResponse<object>(false, "No delivery for this order", null))
                : Ok(new ApiResponse<object>(true, "Success", d));
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId) =>
            Ok(new ApiResponse<object>(true, "Success",
                await svc.GetByUserIdAsync(userId)));

        [HttpGet("tracking/{trackingId}")]
        public async Task<IActionResult> GetTracking(string trackingId)
        {
            var d = await svc.GetTrackingAsync(trackingId);
            return d == null
                ? NotFound(new ApiResponse<object>(false, "Tracking ID not found", null))
                : Ok(new ApiResponse<object>(true, "Success", d));
        }

        [HttpGet("map/all")]
        public async Task<IActionResult> GetAllForMap() =>
            Ok(new ApiResponse<object>(true, "Success",
                await svc.GetAllForMapAsync()));

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateDeliveryDto dto)
        {
            try
            {
                var result = await svc.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
                    new ApiResponse<object>(true, "Delivery created", result));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Create failed");
                return BadRequest(new ApiResponse<object>(false, ex.Message, null));
            }
        }

        // Called by OrderAPI after order placed
        // POST /api/delivery/create-from-order
        [HttpPost("create-from-order")]
        public async Task<IActionResult> CreateFromOrder(
            [FromBody] CreateFromOrderRequest req)
        {
            try
            {
                var result = await svc.CreateFromOrderAsync(req.OrderId, req.UserId);
                return Ok(new ApiResponse<object>(true, "Delivery created", result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CreateFromOrder failed for Order {Id}", req.OrderId);
                return StatusCode(500,
                    new ApiResponse<object>(false, "Internal error", null, ex.Message));
            }
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus(UpdateDeliveryStatusDto dto)
        {
            try
            {
                var result = await svc.UpdateStatusAsync(dto);
                return Ok(new ApiResponse<object>(true, "Status updated", result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(false, ex.Message, null));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(
                    new ApiResponse<object>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UpdateStatus failed for Delivery {Id}", dto.DeliveryId);
                return StatusCode(500,
                    new ApiResponse<object>(false, "Internal error", null, ex.Message));
            }
        }

        [HttpPut("mark-delivered")]
        public async Task<IActionResult> MarkDelivered(MarkDeliveredDto dto)
        {
            try
            {
                var result = await svc.MarkDeliveredAsync(dto);
                return Ok(new ApiResponse<object>(true, "Marked as delivered", result));
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(
                    new ApiResponse<object>(false, ex.Message, null));
            }
        }

        [HttpPut("assign-agent")]
        public async Task<IActionResult> AssignAgent(AssignAgentDto dto)
        {
            try
            {
                var result = await svc.AssignAgentAsync(dto);
                return Ok(new ApiResponse<object>(true, "Agent assigned", result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(false, ex.Message, null));
            }
        }
    }

    public record CreateFromOrderRequest(int OrderId, int UserId);
}