namespace RazorpayApi.Models;

public class CreateRefundRequestDto
{
    public int UserId { get; set; }
    public int OrderId { get; set; }
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string? Reason { get; set; }
}

public class ProcessRefundRequestDto
{
    public int RefundRequestId { get; set; }
    public bool Approve { get; set; }
    public decimal? Amount { get; set; }
    public string? SellerNotes { get; set; }
}
