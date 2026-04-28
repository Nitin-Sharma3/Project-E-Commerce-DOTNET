using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using RazorpayApi.Models;
using RazorpayApi.Services;

namespace RazorpayApi.Pages;

public class PaymentModel : PageModel
{
    private readonly IRazorpayService _razorpayService;
    private readonly ILogger<PaymentModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public string RazorpayKey { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }
    public int AmountRupees { get; private set; } = 100;
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;

    public PaymentModel(
        IRazorpayService razorpayService,
        ILogger<PaymentModel> logger,
        IOptions<RazorpaySettings> settings,
        IHttpClientFactory httpClientFactory)
    {
        _razorpayService = razorpayService;
        _logger = logger;
        RazorpayKey = settings.Value.Key;
        _httpClientFactory = httpClientFactory;
    }

    // GET /Payment — Clear any previous data and render form
    public async Task OnGetAsync([FromQuery] int? userId, [FromQuery] int? orderId, [FromQuery] int? addressId)
    {
        // Clear session to force fresh start
        HttpContext.Session.Clear();
        _logger.LogInformation("Payment page loaded - session cleared");

        if (!userId.HasValue || !orderId.HasValue || !addressId.HasValue)
        {
            AmountRupees = 100;
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}");

            var response = await client.GetFromJsonAsync<PrefillResponse>(
                $"/api/payment/prefill?userId={userId}&orderId={orderId}&addressId={addressId}");

            if (response == null)
            {
                AmountRupees = 100;
                return;
            }

            AmountRupees = (int)Math.Round(response.Amount / 100m, MidpointRounding.AwayFromZero);
            CustomerName = response.Name;
            CustomerEmail = response.Email;
            CustomerPhone = response.Phone;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to prefill payment details");
            AmountRupees = 100;
        }
    }

    private sealed class PrefillResponse
    {
        public int Amount { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    // POST /Payment?handler=CreateOrder — Create order with ACTUAL amount from form
    [HttpPost]
    public IActionResult OnPostCreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            // Validate amount is provided and greater than 0
            if (request.Amount <= 0)
            {
                return BadRequest(new { error = "Amount must be greater than 0." });
            }

            _logger.LogInformation("Creating order with amount: {Amount} paise (₹{AmountInRupees})",
                request.Amount, (decimal)request.Amount / 100);

            // Create order with ACTUAL AMOUNT (not hardcoded 100)
            var response = _razorpayService.CreateOrder(request);

            _logger.LogInformation("Order created: {OrderId}, Amount: ₹{Amount}",
                response.OrderId, (decimal)request.Amount / 100);

            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}