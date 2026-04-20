using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorpayApi.Models;
using RazorpayApi.Services;

namespace RazorpayApi.Pages;

public class RefundModel : PageModel
{
    private readonly IRazorpayService _razorpayService;
    private readonly ILogger<RefundModel> _logger;

    [BindProperty]
    public string PaymentId { get; set; } = string.Empty;

    [BindProperty]
    public int? RefundAmount { get; set; }

    [BindProperty]
    public string? RefundNotes { get; set; }

    public RefundResponse? RefundResponse { get; set; }
    public List<RefundResponse>? PaymentRefunds { get; set; }
    public RefundResponse? RefundStatus { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public string ActiveTab { get; set; } = "create";

    public RefundModel(IRazorpayService razorpayService, ILogger<RefundModel> logger)
    {
        _razorpayService = razorpayService;
        _logger = logger;
    }

    public void OnGet()
    {
        _logger.LogInformation("Refund page loaded");
    }

    // POST handler for full refund
    [HttpPost]
    public IActionResult OnPostFullRefund()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(PaymentId))
            {
                ErrorMessage = "Payment ID is required.";
                ActiveTab = "create";
                return Page();
            }

            _logger.LogInformation("Processing full refund for payment {PaymentId}", PaymentId);

            RefundResponse = _razorpayService.Refund(PaymentId);

            if (RefundResponse.Success)
            {
                SuccessMessage = $"Full refund initiated successfully! Refund ID: {RefundResponse.RefundId}";
                _logger.LogInformation("Full refund successful: {RefundId}", RefundResponse.RefundId);
                PaymentId = string.Empty;
                RefundAmount = null;
                RefundNotes = string.Empty;
            }
            else
            {
                ErrorMessage = RefundResponse.ErrorMessage ?? "Failed to process full refund.";
                _logger.LogError("Full refund failed: {Error}", RefundResponse.ErrorMessage);
            }

            ActiveTab = "create";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during full refund");
            ErrorMessage = $"An error occurred: {ex.Message}";
            ActiveTab = "create";
            return Page();
        }
    }

    // POST handler for partial refund
    [HttpPost]
    public IActionResult OnPostPartialRefund()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(PaymentId))
            {
                ErrorMessage = "Payment ID is required.";
                ActiveTab = "create";
                return Page();
            }

            if (!RefundAmount.HasValue || RefundAmount <= 0)
            {
                ErrorMessage = "Refund amount must be greater than 0.";
                ActiveTab = "create";
                return Page();
            }

            // Convert rupees to paise (multiply by 100)
            int amountInPaise = (int)(RefundAmount.Value * 100);

            _logger.LogInformation("Processing partial refund for payment {PaymentId}, amount ₹{Amount} ({Paise} paise)", PaymentId, RefundAmount.Value, amountInPaise);

            RefundResponse = _razorpayService.RefundPartial(PaymentId, amountInPaise, RefundNotes);

            if (RefundResponse.Success)
            {
                SuccessMessage = $"Partial refund of ₹{RefundAmount.Value:N2} initiated successfully! Refund ID: {RefundResponse.RefundId}";
                _logger.LogInformation("Partial refund successful: {RefundId}", RefundResponse.RefundId);
                PaymentId = string.Empty;
                RefundAmount = null;
                RefundNotes = string.Empty;
            }
            else
            {
                ErrorMessage = RefundResponse.ErrorMessage ?? "Failed to process partial refund.";
                _logger.LogError("Partial refund failed: {Error}", RefundResponse.ErrorMessage);
            }

            ActiveTab = "create";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during partial refund");
            ErrorMessage = $"An error occurred: {ex.Message}";
            ActiveTab = "create";
            return Page();
        }
    }

    // POST handler to retrieve refund status (shows total refunded for a payment)
     [HttpPost]
     public IActionResult OnPostGetRefundStatus(string refundId)
     {
         try
         {
             if (string.IsNullOrWhiteSpace(refundId))
             {
                 ErrorMessage = "Payment ID or Refund ID is required.";
                 ActiveTab = "status";
                 return Page();
             }

             _logger.LogInformation("Fetching refund status for {RefundId}", refundId);

             // Try to get all refunds for the payment ID
             // This will show total refunded amount across all refunds
             var allRefunds = _razorpayService.GetPaymentRefunds(refundId);

             if (allRefunds.Count > 0 && allRefunds[0].Success)
             {
                 // Create a summary response showing total refunded
                 int totalRefunded = allRefunds.Where(r => r.Success && r.Amount.HasValue).Sum(r => r.Amount.Value);
                 string refundStatuses = string.Join(", ", allRefunds.Where(r => r.Success).Select(r => r.Status).Distinct());

                 RefundStatus = new RefundResponse
                 {
                     Success = true,
                     PaymentId = refundId,
                     Amount = totalRefunded,
                     Status = allRefunds.Count == 1 ? allRefunds[0].Status : "Multiple refunds",
                     Message = $"Total refunded amount across {allRefunds.Count} refund(s) for payment {refundId}.",
                     CreatedAt = allRefunds.Where(r => r.Success).Max(r => r.CreatedAt)
                 };

                 SuccessMessage = $"Found {allRefunds.Count} refund(s) for payment {refundId}. Total refunded: ₹{(totalRefunded / 100m):N2}";
                 _logger.LogInformation("Refund summary retrieved: PaymentId {PaymentId}, TotalAmount {Total}, RefundCount {Count}", 
                     refundId, totalRefunded, allRefunds.Count);
             }
             else
             {
                 ErrorMessage = $"No refunds found for payment ID {refundId}.";
                 _logger.LogWarning("No refunds found for payment ID: {PaymentId}", refundId);
             }

             ActiveTab = "status";
             return Page();
         }
         catch (Exception ex)
         {
             _logger.LogError(ex, "Exception while fetching refund status");
             ErrorMessage = $"An error occurred: {ex.Message}";
             ActiveTab = "status";
             return Page();
         }
     }

    // GET handler to retrieve all refunds for a payment
    [HttpPost]
    public IActionResult OnPostGetPaymentRefunds()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(PaymentId))
            {
                ErrorMessage = "Payment ID is required.";
                ActiveTab = "history";
                return Page();
            }

            _logger.LogInformation("Fetching all refunds for payment {PaymentId}", PaymentId);

            PaymentRefunds = _razorpayService.GetPaymentRefunds(PaymentId);

            if (PaymentRefunds != null && PaymentRefunds.Count > 0)
            {
                var successRefunds = PaymentRefunds.Where(r => r.Success).ToList();
                if (successRefunds.Any())
                {
                    SuccessMessage = $"Found {successRefunds.Count} refund(s) for this payment.";
                    _logger.LogInformation("Retrieved {Count} refunds for payment {PaymentId}", successRefunds.Count, PaymentId);
                }
                else
                {
                    ErrorMessage = PaymentRefunds.First().ErrorMessage ?? "No refunds found for this payment.";
                }
            }
            else
            {
                ErrorMessage = "No refunds found for this payment.";
            }

            ActiveTab = "history";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while fetching payment refunds");
            ErrorMessage = $"An error occurred: {ex.Message}";
            ActiveTab = "history";
            return Page();
        }
    }
}
