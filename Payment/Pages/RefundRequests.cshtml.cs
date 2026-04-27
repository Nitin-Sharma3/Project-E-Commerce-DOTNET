using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorpayApi.Data;
using RazorpayApi.Models;
using RazorpayApi.Services;

namespace RazorpayApi.Pages;

public class RefundRequestsModel : PageModel
{
    private readonly PaymentDbContext _dbContext;
    private readonly IRazorpayService _razorpayService;
    private readonly ILogger<RefundRequestsModel> _logger;

    public List<RefundRequestRecord> Requests { get; private set; } = new();

    [BindProperty]
    public int RefundRequestId { get; set; }

    [BindProperty]
    public bool Approve { get; set; }

    [BindProperty]
    public decimal? Amount { get; set; }

    [BindProperty]
    public string? SellerNotes { get; set; }

    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public RefundRequestsModel(PaymentDbContext dbContext, IRazorpayService razorpayService, ILogger<RefundRequestsModel> logger)
    {
        _dbContext = dbContext;
        _razorpayService = razorpayService;
        _logger = logger;
    }

    public void OnGet()
    {
        LoadRequests();
    }

    public IActionResult OnPostProcess()
    {
        var record = _dbContext.RefundRequests.FirstOrDefault(r => r.Id == RefundRequestId);
        if (record == null)
        {
            ErrorMessage = "Refund request not found.";
            LoadRequests();
            return Page();
        }

        if (!string.Equals(record.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = "Only pending refund requests can be processed.";
            LoadRequests();
            return Page();
        }

        record.SellerNotes = SellerNotes;
        record.ProcessedAt = DateTime.UtcNow;

        if (!Approve)
        {
            record.Status = "Rejected";
            _dbContext.SaveChanges();
            SuccessMessage = "Refund request rejected.";
            LoadRequests();
            return Page();
        }

        var refundAmount = Amount.HasValue && Amount.Value > 0m ? Amount.Value : record.Amount;
        var refundResponse = _razorpayService.RefundPartial(record.RazorpayPaymentId, (int)Math.Round(refundAmount), record.Reason);

        if (!refundResponse.Success)
        {
            record.Status = "Failed";
            _dbContext.SaveChanges();
            ErrorMessage = refundResponse.ErrorMessage ?? "Refund failed.";
            LoadRequests();
            return Page();
        }

        record.Status = "Approved";
        record.RazorpayRefundId = refundResponse.RefundId;
        _dbContext.SaveChanges();

        SuccessMessage = "Refund approved and processed successfully.";
        LoadRequests();
        return Page();
    }

    private void LoadRequests()
    {
        Requests = _dbContext.RefundRequests
            .OrderByDescending(r => r.RequestedAt)
            .ToList();
    }
}
