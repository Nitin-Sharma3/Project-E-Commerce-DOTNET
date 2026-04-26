using Microsoft.AspNetCore.Mvc;
using RazorpayApi.Data;
using RazorpayApi.Models;
using RazorpayApi.Services;
using System.Net.Http.Json;

namespace RazorpayApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IRazorpayService _razorpayService;
    private readonly IInvoiceService _invoiceService;
    private readonly PaymentDbContext _dbContext;
    private readonly ILogger<PaymentController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public PaymentController(
        IRazorpayService razorpayService,
        IInvoiceService invoiceService,
        PaymentDbContext dbContext,
        ILogger<PaymentController> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _razorpayService = razorpayService;
        _invoiceService = invoiceService;
        _dbContext = dbContext;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
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
            if (request.UserId.HasValue && request.UserId <= 0)
            {
                return BadRequest(new { error = "UserId must be greater than 0." });
            }

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
    /// Gets prefill details (amount, name, email, phone) from Cart/Order/Address APIs.
    /// </summary>
    [HttpGet("prefill")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPrefill([FromQuery] int userId, [FromQuery] int orderId, [FromQuery] int addressId)
    {
        if (userId <= 0 || orderId <= 0 || addressId <= 0)
        {
            return BadRequest(new { error = "userId, orderId, and addressId are required." });
        }

        try
        {
            var (cart, order, address) = await GetOrderContextAsync(userId, orderId, addressId);
            var totalAmount = cart.TotalAmount > 0m
                ? cart.TotalAmount
                : cart.Items.Sum(item => item.Quantity * item.UnitPrice);
            var amount = (int)Math.Round(totalAmount * 100m, MidpointRounding.AwayFromZero);

            var response = new PrefillResponse
            {
                UserId = userId,
                OrderId = order.Id,
                AddressId = addressId,
                Amount = amount,
                Currency = "INR",
                Name = address.FullName,
                Email = address.Email,
                Phone = address.Phone
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prefill data for user {UserId}", userId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private async Task<(CartApiCartDto Cart, OrderApiOrderDto Order, AddressApiAddressDto Address)> GetOrderContextAsync(int userId, int orderId, int addressId)
    {
        var cartBaseUrl = ResolveBaseUrl(_configuration["CartAPI:BaseUrl"]);
        var orderBaseUrl = ResolveBaseUrl(_configuration["OrderAPI:BaseUrl"]);
        var addressBaseUrl = ResolveBaseUrl(_configuration["AddressAPI:BaseUrl"]);

        var cartClient = _httpClientFactory.CreateClient("CartAPI");
        var orderClient = _httpClientFactory.CreateClient("OrderAPI");
        var addressClient = _httpClientFactory.CreateClient("AddressAPI");

        var cartTask = cartClient.GetFromJsonAsync<CartApiCartDto>(
            new Uri(new Uri(cartBaseUrl), $"api/users/{userId}/cart"));
        var orderTask = orderClient.GetFromJsonAsync<OrderApiOrderDto>(
            new Uri(new Uri(orderBaseUrl), $"api/users/{userId}/orders/{orderId}"));
        var addressTask = addressClient.GetFromJsonAsync<AddressApiAddressDto>(
            new Uri(new Uri(addressBaseUrl), $"api/users/{userId}/addresses/{addressId}"));

        await Task.WhenAll(cartTask, orderTask, addressTask);

        var cart = cartTask.Result ?? throw new InvalidOperationException("Cart not found for the user.");
        var order = orderTask.Result ?? throw new InvalidOperationException("Order not found for the user.");
        var address = addressTask.Result ?? throw new InvalidOperationException("Address not found for the user.");

        return (cart, order, address);
    }

    private bool IsMockApisEnabled()
    {
        return bool.TryParse(_configuration["MockApis:Enabled"], out var enabled) && enabled;
    }

    private CartApiCartDto BuildMockCart(int userId)
    {
        return new CartApiCartDto
        {
            Id = 1,
            UserId = userId,
            Items = new List<CartApiItemDto>
            {
                new()
                {
                    Id = 1,
                    ProductId = 101,
                    Quantity = 2,
                    UnitPrice = 499.50m
                },
                new()
                {
                    Id = 2,
                    ProductId = 202,
                    Quantity = 1,
                    UnitPrice = 299.00m
                }
            }
        };
    }

    private OrderApiOrderDto BuildMockOrder(int userId, int orderId)
    {
        return new OrderApiOrderDto
        {
            Id = orderId,
            UserId = userId,
            TotalAmount = 1298.00m,
            ShippingAddress = new OrderApiAddressDto
            {
                FullName = "Test User",
                Phone = "+91-9999999999",
                AddressLine1 = "123 Test Street",
                City = "Bengaluru",
                State = "KA",
                PostalCode = "560001",
                Country = "India"
            }
        };
    }

    private AddressApiAddressDto BuildMockAddress(int userId, int addressId)
    {
        return new AddressApiAddressDto
        {
            Id = addressId,
            UserId = userId,
            FullName = "Test User",
            Email = "test.user@example.com",
            Phone = "+91-9999999999",
            AddressLine1 = "123 Test Street",
            City = "Bengaluru",
            State = "KA",
            PostalCode = "560001",
            Country = "India"
        };
    }

    private string ResolveBaseUrl(string? configuredBaseUrl)
    {
        var requestBaseUrl = $"{Request.Scheme}://{Request.Host}/";

        if (string.IsNullOrWhiteSpace(configuredBaseUrl))
        {
            return requestBaseUrl;
        }

        if (Uri.TryCreate(configuredBaseUrl, UriKind.Absolute, out var configuredUri)
            && string.Equals(configuredUri.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase)
            && configuredUri.Port != Request.Host.Port)
        {
            return requestBaseUrl;
        }

        return configuredBaseUrl;
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

    /// <summary>
    /// Creates a Razorpay order by pulling name, email, phone, amount and items from Cart/Order APIs
    /// and address from Address API, then maps order id as external order id.
    /// </summary>
    [HttpPost("create-order-from-apis")]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrderFromApis([FromBody] CreateOrderRequest request)
    {
        if (!request.UserId.HasValue || request.UserId <= 0)
        {
            return BadRequest(new { error = "UserId is required." });
        }

        if (!request.OrderId.HasValue || request.OrderId <= 0)
        {
            return BadRequest(new { error = "OrderId is required." });
        }

        if (!request.AddressId.HasValue || request.AddressId <= 0)
        {
            return BadRequest(new { error = "AddressId is required." });
        }

        try
        {
            var userId = request.UserId.Value;
            var orderId = request.OrderId.Value;
            var addressId = request.AddressId.Value;

            var (cart, order, address) = await GetOrderContextAsync(userId, orderId, addressId);

            var totalAmount = cart.TotalAmount > 0m
                ? cart.TotalAmount
                : cart.Items.Sum(item => item.Quantity * item.UnitPrice);
            request.Amount = (int)Math.Round(totalAmount * 100m, MidpointRounding.AwayFromZero);
            request.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "INR" : request.Currency;
            request.Receipt = string.IsNullOrWhiteSpace(request.Receipt)
                ? $"order_{order.Id}"
                : request.Receipt;
            request.ExternalOrderId = order.Id.ToString();

            request.Items = cart.Items.Select(item => new OrderItem
            {
                ItemName = $"Product #{item.ProductId}",
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Description = "Cart item"
            }).ToList();

            var customerName = address.FullName;
            var customerPhone = address.Phone;
            var customerEmail = address.Email;

            request.CustomerDetails = new CustomerDetails
            {
                Name = customerName,
                Email = customerEmail,
                Phone = customerPhone,
                Address = address.AddressLine1,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode
            };

            var response = _razorpayService.CreateOrder(request);
            var transaction = new PaymentTransaction
            {
                UserId = userId,
                OrderId = order.Id,
                RazorpayOrderId = response.OrderId,
                Amount = request.Amount,
                Currency = request.Currency,
                Receipt = request.Receipt,
                CustomerName = request.CustomerDetails.Name,
                CustomerEmail = request.CustomerDetails.Email,
                CustomerPhone = request.CustomerDetails.Phone,
                CustomerAddress = request.CustomerDetails.Address,
                CustomerCity = request.CustomerDetails.City,
                CustomerState = request.CustomerDetails.State,
                CustomerPostalCode = request.CustomerDetails.PostalCode,
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.PaymentTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Order created from APIs: {OrderId} (external {ExternalOrderId})", response.OrderId, response.ExternalOrderId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Razorpay order from APIs");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private sealed class CartApiCartDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<CartApiItemDto> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

    private sealed class CartApiItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    private sealed class OrderApiOrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderApiAddressDto ShippingAddress { get; set; } = new();
    }

    private sealed class OrderApiAddressDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    private sealed class AddressApiAddressDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    private sealed class PrefillResponse
    {
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public int AddressId { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }


}