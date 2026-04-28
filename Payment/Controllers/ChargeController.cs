using Microsoft.AspNetCore.Mvc;
using RazorpayApi.Data;
using RazorpayApi.Models;
using RazorpayApi.Services;

namespace RazorpayApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChargeController : ControllerBase
{
    private readonly IRazorpayService _razorpayService;
    private readonly PaymentDbContext _dbContext;
    private readonly ILogger<ChargeController> _logger;

    public ChargeController(
        IRazorpayService razorpayService,
        PaymentDbContext dbContext,
        ILogger<ChargeController> logger)
    {
        _razorpayService = razorpayService;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost("verify")]
    [ProducesResponseType(typeof(ChargeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult VerifyPayment([FromBody] ChargeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RazorpayPaymentId) ||
            string.IsNullOrWhiteSpace(request.RazorpayOrderId) ||
            string.IsNullOrWhiteSpace(request.RazorpaySignature))
        {
            return BadRequest(new { error = "razorpay_payment_id, razorpay_order_id and razorpay_signature are required." });
        }

        try
        {
            var response = _razorpayService.VerifyAndCharge(request);
            var transaction = _dbContext.PaymentTransactions
                .FirstOrDefault(t => t.RazorpayOrderId == request.RazorpayOrderId);

            if (transaction != null)
            {
                transaction.RazorpayPaymentId = request.RazorpayPaymentId;
                transaction.RazorpaySignature = request.RazorpaySignature;
                transaction.PaidAt = DateTime.UtcNow;
                transaction.Status = response.Success ? "Paid" : "Failed";
                _dbContext.SaveChanges();
            }

            _logger.LogInformation("Payment verified: {PaymentId}", response.PaymentId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Signature verification failed for payment {PaymentId}", request.RazorpayPaymentId);
            return BadRequest(new { error = "Payment verification failed.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Initiates a full refund for the given payment ID.
    /// </summary>
    /// <param name="paymentId">Payment ID to refund</param>
    /// <returns>Refund details</returns>
    [HttpPost("{paymentId}/refund")]
    [ProducesResponseType(typeof(RefundResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Refund(string paymentId)
    {
        if (string.IsNullOrWhiteSpace(paymentId))
        {
            return BadRequest(new { error = "paymentId is required." });
        }

        try
        {
            var response = _razorpayService.Refund(paymentId);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            _logger.LogInformation("Full refund initiated for payment {PaymentId}, refund {RefundId}",
                paymentId, response.RefundId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Full refund failed for payment {PaymentId}", paymentId);
            return StatusCode(500, new RefundResponse
            {
                Success = false,
                PaymentId = paymentId,
                ErrorMessage = ex.Message,
                Message = "Full refund failed."
            });
        }
    }

    [HttpPost("{paymentId}/refund/partial")]
    [ProducesResponseType(typeof(RefundResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult RefundPartial(string paymentId, [FromBody] PartialRefundRequest request)
    {
        if (string.IsNullOrWhiteSpace(paymentId))
        {
            return BadRequest(new { error = "paymentId is required." });
        }

        if (request?.Amount <= 0)
        {
            return BadRequest(new { error = "Amount must be greater than 0." });
        }

        try
        {
            var response = _razorpayService.RefundPartial(paymentId, request.Amount, request.Notes);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            _logger.LogInformation("Partial refund initiated for payment {PaymentId}, amount {Amount}, refund {RefundId}",
                paymentId, request.Amount, response.RefundId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Partial refund failed for payment {PaymentId}, amount {Amount}",
                paymentId, request?.Amount);
            return StatusCode(500, new RefundResponse
            {
                Success = false,
                PaymentId = paymentId,
                Amount = request?.Amount,
                ErrorMessage = ex.Message,
                Message = "Partial refund failed."
            });
        }
    }

    [HttpPost("refund/idempotent")]
    [ProducesResponseType(typeof(RefundResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RefundResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult RefundIdempotent([FromBody] RefundRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.PaymentId))
        {
            return BadRequest(new { error = "PaymentId is required." });
        }

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            return BadRequest(new { error = "IdempotencyKey is required for idempotent requests." });
        }

        if (request.Amount.HasValue && request.Amount.Value <= 0)
        {
            return BadRequest(new { error = "Amount must be greater than 0 if specified." });
        }

        try
        {
            var response = _razorpayService.RefundIdempotent(request);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            var refundType = request.Amount.HasValue ? "Partial" : "Full";
            _logger.LogInformation("{RefundType} idempotent refund for payment {PaymentId}, idempotency {IdempotencyKey}, refund {RefundId}",
                refundType, request.PaymentId, request.IdempotencyKey, response.RefundId);

            return CreatedAtAction(nameof(GetRefundStatus), new { refundId = response.RefundId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Idempotent refund failed for payment {PaymentId}, idempotency {IdempotencyKey}",
                request.PaymentId, request.IdempotencyKey);
            return StatusCode(500, new RefundResponse
            {
                Success = false,
                PaymentId = request.PaymentId,
                ErrorMessage = ex.Message,
                Message = "Idempotent refund failed."
            });
        }
    }

    [HttpGet("refund/{refundId}/status")]
    [ProducesResponseType(typeof(RefundResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetRefundStatus(string refundId)
    {
        if (string.IsNullOrWhiteSpace(refundId))
        {
            return BadRequest(new { error = "refundId is required." });
        }

        try
        {
            var response = _razorpayService.GetRefundStatus(refundId);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            _logger.LogInformation("Refund status retrieved for {RefundId}, status {Status}",
                refundId, response.Status);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve refund status for {RefundId}", refundId);
            return StatusCode(500, new RefundResponse
            {
                Success = false,
                RefundId = refundId,
                ErrorMessage = ex.Message,
                Message = "Failed to retrieve refund status."
            });
        }
    }
    [HttpGet("{paymentId}/refunds")]
    [ProducesResponseType(typeof(List<RefundResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetPaymentRefunds(string paymentId)
    {
        if (string.IsNullOrWhiteSpace(paymentId))
        {
            return BadRequest(new { error = "paymentId is required." });
        }

        try
        {
            var refunds = _razorpayService.GetPaymentRefunds(paymentId);

            if (refunds.Any(r => !r.Success))
            {
                return StatusCode(500, refunds);
            }

            _logger.LogInformation("Retrieved {RefundCount} refunds for payment {PaymentId}",
                refunds.Count, paymentId);
            return Ok(refunds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve refunds for payment {PaymentId}", paymentId);
            return StatusCode(500, new
            {
                error = "Failed to retrieve refunds.",
                detail = ex.Message
            });
        }
    }

    [HttpPost("refund-requests")]
    [ProducesResponseType(typeof(RefundRequestRecord), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateRefundRequest([FromBody] CreateRefundRequestDto request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required." });
        }

        if (string.IsNullOrWhiteSpace(request.RazorpayPaymentId) || string.IsNullOrWhiteSpace(request.RazorpayOrderId))
        {
            return BadRequest(new { error = "RazorpayPaymentId and RazorpayOrderId are required." });
        }

        if (request.Amount <= 0)
        {
            return BadRequest(new { error = "Amount must be greater than 0." });
        }

        var record = new RefundRequestRecord
        {
            UserId = request.UserId,
            OrderId = request.OrderId,
            RazorpayOrderId = request.RazorpayOrderId,
            RazorpayPaymentId = request.RazorpayPaymentId,
            Amount = request.Amount,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "INR" : request.Currency,
            Reason = request.Reason,
            Status = "Pending",
            RequestedAt = DateTime.UtcNow
        };

        _dbContext.RefundRequests.Add(record);
        _dbContext.SaveChanges();

        return CreatedAtAction(nameof(GetRefundRequestById), new { refundRequestId = record.Id }, record);
    }

    [HttpGet("refund-requests")]
    [ProducesResponseType(typeof(List<RefundRequestRecord>), StatusCodes.Status200OK)]
    public IActionResult GetRefundRequests([FromQuery] string? status)
    {
        var query = _dbContext.RefundRequests.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status);
        }

        var results = query.OrderByDescending(r => r.RequestedAt).ToList();
        return Ok(results);
    }

    [HttpGet("refund-requests/{refundRequestId:int}")]
    [ProducesResponseType(typeof(RefundRequestRecord), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetRefundRequestById(int refundRequestId)
    {
        var record = _dbContext.RefundRequests.FirstOrDefault(r => r.Id == refundRequestId);
        if (record == null)
        {
            return NotFound(new { error = "Refund request not found." });
        }

        return Ok(record);
    }

    [HttpPost("refund-requests/process")]
    [ProducesResponseType(typeof(RefundRequestRecord), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ProcessRefundRequest([FromBody] ProcessRefundRequestDto request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required." });
        }

        var record = _dbContext.RefundRequests.FirstOrDefault(r => r.Id == request.RefundRequestId);
        if (record == null)
        {
            return NotFound(new { error = "Refund request not found." });
        }

        if (!string.Equals(record.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Only pending refund requests can be processed." });
        }

        record.SellerNotes = request.SellerNotes;
        record.ProcessedAt = DateTime.UtcNow;

        if (!request.Approve)
        {
            record.Status = "Rejected";
            _dbContext.SaveChanges();
            return Ok(record);
        }

        var refundAmount = request.Amount.HasValue && request.Amount.Value > 0 ? request.Amount.Value : record.Amount;
        var refundResponse = _razorpayService.RefundPartial(record.RazorpayPaymentId, (int)Math.Round(refundAmount), record.Reason);

        if (!refundResponse.Success)
        {
            record.Status = "Failed";
            _dbContext.SaveChanges();
            return StatusCode(500, new { error = refundResponse.ErrorMessage ?? "Refund failed." });
        }

        record.Status = "Approved";
        record.RazorpayRefundId = refundResponse.RefundId;
        _dbContext.SaveChanges();

        return Ok(record);
    }
}