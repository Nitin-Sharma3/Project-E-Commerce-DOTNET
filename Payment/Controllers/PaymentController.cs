using Microsoft.AspNetCore.Mvc;
using RazorpayApi.Models;
using RazorpayApi.Services;

namespace RazorpayApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IRazorpayService _razorpayService;
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IRazorpayService razorpayService,
        IInvoiceService invoiceService,
        ILogger<PaymentController> logger)
    {
        _razorpayService = razorpayService;
        _invoiceService = invoiceService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a Razorpay order and returns the order ID.
    /// The client uses this order ID to open the Razorpay checkout modal.
    /// 
    /// Supports extended payload with items, seller/customer details, tax, and discounts.
    /// </summary>
    [HttpPost("create-order")]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var response = _razorpayService.CreateOrder(request);
            _logger.LogInformation("Order created: {OrderId}", response.OrderId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Razorpay order");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generates and downloads an invoice/receipt as PDF after successful payment.
    /// Call this endpoint after payment verification with order and payment details.
    /// </summary>
    [HttpPost("generate-invoice")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GenerateInvoice([FromBody] InvoiceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OrderId) || string.IsNullOrWhiteSpace(request.PaymentId))
        {
            return BadRequest(new { error = "OrderId and PaymentId are required." });
        }

        try
        {
            var response = _invoiceService.GenerateInvoicePdf(request);

            if (!response.Success || response.PdfBytes == null)
            {
                return StatusCode(500, new { error = "Failed to generate invoice." });
            }

            _logger.LogInformation("Invoice generated for order {OrderId}", request.OrderId);

            // Return PDF file for download
            return File(response.PdfBytes, "application/pdf", response.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice for order {OrderId}", request.OrderId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves stored payment details for a given order ID (for reference/preview).
    /// This assumes you store payment details in a database or cache.
    /// For now, this is a placeholder that can be implemented with your storage mechanism.
    /// </summary>
    [HttpGet("details/{orderId}")]
    [ProducesResponseType(typeof(PaymentDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetPaymentDetails(string orderId)
    {
        try
        {
            // TODO: Implement actual retrieval from database or cache
            // For now, return a placeholder response

            var response = new PaymentDetailsResponse
            {
                OrderId = orderId,
                PaymentId = string.Empty,
                Amount = 0m,
                Currency = "INR",
                Items = new(),
                TransactionDate = DateTime.Now
            };

            _logger.LogInformation("Payment details retrieved for order {OrderId}", orderId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment details for order {OrderId}", orderId);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}