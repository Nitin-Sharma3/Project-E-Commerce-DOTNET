using DeliveryService.DTOs;
using DeliveryService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryController(
        IDeliveryService svc,
        IHttpClientFactory http,
        ILogger<DeliveryController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(new ApiResponse<object>(true, "Deliveries fetched", await svc.GetAllAsync()));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var d = await svc.GetByIdAsync(id);
            return d == null
                ? NotFound(new ApiResponse<object>(false, $"Delivery {id} not found", null))
                : Ok(new ApiResponse<object>(true, "Success", d));
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId) =>
            Ok(new ApiResponse<object>(true, "Success", await svc.GetByUserIdAsync(userId)));

        // ── NEW: find delivery by OrderId ─────────────────────────────────
        [HttpGet("order/{orderId:int}")]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            var d = await svc.GetByOrderIdAsync(orderId);
            return d == null
                ? NotFound(new ApiResponse<object>(false, "No delivery for this order", null))
                : Ok(new ApiResponse<object>(true, "Success", d));
        }

        [HttpGet("tracking/{trackingId}")]
        public async Task<IActionResult> GetTracking(string trackingId)
        {
            var data = await svc.GetTrackingAsync(trackingId);
            return data == null
                ? NotFound(new ApiResponse<object>(false, "Tracking ID not found", null))
                : Ok(new ApiResponse<object>(true, "Success", data));
        }

        [HttpGet("map/all")]
        public async Task<IActionResult> GetAllForMap() =>
            Ok(new ApiResponse<object>(true, "Map data fetched", await svc.GetAllForMapAsync()));

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

        // ── NEW: Called by OrderAPI after order placed ─────────────────────
        [HttpPost("create-from-order/{orderId:int}")]
        public async Task<IActionResult> CreateFromOrder(int orderId)
        {
            try
            {
                // Check not already created
                var existing = await svc.GetByOrderIdAsync(orderId);
                if (existing != null)
                    return Ok(new ApiResponse<object>(true, "Delivery already exists", existing));

                // Fetch order from OrderAPI
                var orderClient = http.CreateClient("OrderService");
                var orderResp = await orderClient.GetAsync(
                    $"api/users/0/orders/{orderId}");

                // OrderAPI GetById (admin-style, no userId filter)
                // Try the direct endpoint
                var resp = await orderClient.GetAsync($"api/orders/{orderId}");

                // If 404, fall back with orderId only approach
                if (!resp.IsSuccessStatusCode)
                {
                    logger.LogWarning("Could not fetch Order {Id} from OrderAPI: {Code}",
                        orderId, resp.StatusCode);
                    return BadRequest(new ApiResponse<object>(false,
                        $"Order {orderId} not found in OrderAPI", null));
                }

                var order = await resp.Content
                    .ReadFromJsonAsync<OrderApiResponseDto>(new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (order == null)
                    return BadRequest(new ApiResponse<object>(false, "Could not parse order", null));

                // Map OrderAPI fields → CreateDeliveryDto
                var dto = new CreateDeliveryDto(
                    order.Id,
                    order.UserId,
                    order.ShippingAddress.FullName,        // FullName → RecipientName
                    order.ShippingAddress.AddressLine1,
                    order.ShippingAddress.AddressLine2,
                    order.ShippingAddress.City,
                    order.ShippingAddress.State,
                    order.ShippingAddress.PostalCode,      // PostalCode → Pincode
                    order.ShippingAddress.Country,
                    order.ShippingAddress.Phone,           // Phone → ContactPhone
                    null,                                  // no email in OrderAPI
                    order.Items.Select(i => new DeliveryItemDto(
                        i.ProductId,
                        i.ProductName,
                        i.Quantity,
                        i.UnitPrice,
                        i.Subtotal,
                        null                               // no ImageUrl in OrderAPI
                    )).ToList()
                );

                var result = await svc.CreateAsync(dto);
                logger.LogInformation("Delivery created from Order {Id}", orderId);
                return Ok(new ApiResponse<object>(true, "Delivery created from order", result));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CreateFromOrder failed for OrderId {Id}", orderId);
                return StatusCode(500, new ApiResponse<object>(false, ex.Message, null));
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