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
}