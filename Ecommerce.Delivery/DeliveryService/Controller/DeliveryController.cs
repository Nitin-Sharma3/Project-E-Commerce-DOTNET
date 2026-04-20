using DeliveryService.DTOs;
using DeliveryService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    //[ApiController, Route("api/delivery")]
    public class DeliveryController(IDeliveryService svc, ILogger<DeliveryController> logger)
     : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await svc.GetAllAsync();
            return Ok(new ApiResponse<object>(true, "Deliveries fetched", data));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var d = await svc.GetByIdAsync(id);
            return d == null
                ? NotFound(new ApiResponse<object>(false, $"Delivery {id} not found", null))
                : Ok(new ApiResponse<object>(true, "Success", d));
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var data = await svc.GetByUserIdAsync(userId);
            return Ok(new ApiResponse<object>(true, "Success", data));
        }

        [HttpGet("tracking/{trackingId}")]
        public async Task<IActionResult> GetTracking(string trackingId)
        {
            var data = await svc.GetTrackingAsync(trackingId);
            return data == null
                ? NotFound(new ApiResponse<object>(false, "Tracking ID not found", null))
                : Ok(new ApiResponse<object>(true, "Success", data));
        }

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
                logger.LogError(ex, "Create delivery failed");
                return BadRequest(new ApiResponse<object>(false, ex.Message, null));
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
                return UnprocessableEntity(new ApiResponse<object>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Status update failed for delivery {Id}", dto.DeliveryId);
                return StatusCode(500, new ApiResponse<object>(false, "Internal error", null, ex.Message));
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
                return UnprocessableEntity(new ApiResponse<object>(false, ex.Message, null));
            }
        }
    }
}
