using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorpayApi.Data;
using RazorpayApi.Models;
using RazorpayApi.Services;

namespace RazorpayApi.Pages;

public class ChargeModel : PageModel
{
    private readonly IRazorpayService _razorpayService;
    private readonly ILogger<ChargeModel> _logger;
    private readonly PaymentDbContext _dbContext;

    public bool Success { get; private set; }
    public string PaymentId { get; private set; } = string.Empty;
    public string OrderId { get; private set; } = string.Empty;
    public string ExternalOrderId { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;
    public string CustomerAddress { get; private set; } = string.Empty;
    public string CustomerCity { get; private set; } = string.Empty;
    public string CustomerState { get; private set; } = string.Empty;
    public string CustomerPostalCode { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "INR";
    public DateTime TransactionDate { get; private set; } = DateTime.Now;
    public string? ErrorMessage { get; private set; }
    public string? RefundId { get; private set; }

    // Invoice-related properties (can be extended based on stored data)
    public List<OrderItem> Items { get; private set; } = new();
    public decimal Subtotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }

    public ChargeModel(IRazorpayService razorpayService, ILogger<ChargeModel> logger, PaymentDbContext dbContext)
    {
        _razorpayService = razorpayService;
        _logger = logger;
        _dbContext = dbContext;
    }

    // GET /Charge — not used directly; redirect to Payment
    public IActionResult OnGet() => RedirectToPage("/Payment");

    // POST /Charge — called by the hidden form after Razorpay checkout
    public void OnPost(
        string razorpayPaymentId,
        string razorpayOrderId,
        string razorpaySignature,
        decimal amount = 1.00m)
    {
        PaymentId = razorpayPaymentId;
        OrderId = razorpayOrderId;
        TransactionDate = DateTime.Now;

        var transaction = _dbContext.PaymentTransactions
            .FirstOrDefault(t => t.RazorpayOrderId == razorpayOrderId);
        ExternalOrderId = transaction?.OrderId.ToString() ?? string.Empty;
        CustomerName = transaction?.CustomerName ?? string.Empty;
        CustomerEmail = transaction?.CustomerEmail ?? string.Empty;
        CustomerPhone = transaction?.CustomerPhone ?? string.Empty;
        CustomerAddress = transaction?.CustomerAddress ?? string.Empty;
        CustomerCity = transaction?.CustomerCity ?? string.Empty;
        CustomerState = transaction?.CustomerState ?? string.Empty;
        CustomerPostalCode = transaction?.CustomerPostalCode ?? string.Empty;

        try
        {
            var response = _razorpayService.VerifyAndCharge(new ChargeRequest
            {
                RazorpayPaymentId = razorpayPaymentId,
                RazorpayOrderId = razorpayOrderId,
                RazorpaySignature = razorpaySignature
            });

            Success = response.Success;

            // Set amounts from the submitted form or fetch from DB
            Amount = amount; 
            Currency = "INR";

            // Initialize sample items (in production, fetch from database based on OrderId)
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ItemName = "Service/Product",
                    Quantity = 1,
                    UnitPrice = amount,
                    Description = "Payment for order #" + OrderId
                }
            };

            Subtotal = Amount;
            TaxAmount = 0m;
            DiscountAmount = 0m;

            _logger.LogInformation("Payment verified: {PaymentId}", PaymentId);
        }
        catch (Exception ex)
        {
            Success = false;
            ErrorMessage = ex.Message;
            _logger.LogError(ex, "Signature verification failed for {PaymentId}", PaymentId);
        }
    }

    // POST /Charge?handler=Refund — refund button on the result page
    public void OnPostRefund(string paymentId)
    {
        PaymentId = paymentId;

        try
        {
            var response = _razorpayService.Refund(paymentId);
            Success = true;
            RefundId = response.RefundId;
            _logger.LogInformation("Refund {RefundId} for payment {PaymentId}", RefundId, PaymentId);
        }
        catch (Exception ex)
        {
            Success = false;
            ErrorMessage = $"Refund failed: {ex.Message}";
        }
    }
}