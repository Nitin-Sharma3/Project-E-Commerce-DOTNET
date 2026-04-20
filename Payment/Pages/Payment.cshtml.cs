using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using RazorpayApi.Models;
using RazorpayApi.Services;

namespace RazorpayApi.Pages;

public class PaymentModel : PageModel
{
    private readonly IRazorpayService _razorpayService;
    private readonly ILogger<PaymentModel> _logger;

    public string RazorpayKey { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }

    public PaymentModel(
        IRazorpayService razorpayService,
        ILogger<PaymentModel> logger,
        IOptions<RazorpaySettings> settings)
    {
        _razorpayService = razorpayService;
        _logger = logger;
        RazorpayKey = settings.Value.Key;
    }

    // GET /Payment — Clear any previous data and render form
    public void OnGet()
    {
        // Clear session to force fresh start
        HttpContext.Session.Clear();
        _logger.LogInformation("Payment page loaded - session cleared");
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