using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
    using System.Text;
    using System.Text.Json;
    using UserDashboardMVC.ViewModels;

namespace UserDashboardMVC.Controllers
{
  
    
    public class OrderController : Controller
    {
        private readonly HttpClient _orderClient;
        private readonly ILogger<OrderController> _logger;
        private const int CurrentUserId = 1;

        private static readonly JsonSerializerOptions JsonOpts =
            new() { PropertyNameCaseInsensitive = true };

        public OrderController(IHttpClientFactory factory, ILogger<OrderController> logger)
        {
            _orderClient = factory.CreateClient("OrderAPI");
            _logger = logger;
        }

        // ── GET /Order — list all orders ─────────────────────
        public async Task<IActionResult> Index(string filter = "All")
        {
            var model = new OrderListPageViewModel
            {
                UserId = CurrentUserId,
                ActiveFilter = filter,
                SuccessMessage = TempData["Success"] as string,
                ErrorMessage = TempData["Error"] as string
            };

            try
            {
                var resp = await _orderClient.GetAsync($"api/users/{CurrentUserId}/orders");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiWrapper<List<OrderSummaryResponse>>>(json, JsonOpts);
                    if (result?.Data != null)
                    {
                        model.Orders = result.Data.Select(o => new OrderSummaryViewModel
                        {
                            Id = o.Id,
                            OrderDate = o.OrderDate,
                            Status = o.Status,
                            TotalAmount = o.TotalAmount,
                            TotalItems = o.TotalItems
                        }).ToList();
                    }
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Failed to load orders"); }

            return View(model);
        }

        // ── GET /Order/Detail/{id} — single order ────────────
        public async Task<IActionResult> Detail(int id)
        {
            var model = new OrderDetailPageViewModel
            {
                SuccessMessage = TempData["Success"] as string,
                ErrorMessage = TempData["Error"] as string
            };

            try
            {
                var resp = await _orderClient.GetAsync($"api/users/{CurrentUserId}/orders/{id}");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiWrapper<OrderDetailResponse>>(json, JsonOpts);
                    if (result?.Data != null)
                    {
                        var o = result.Data;
                        model.Order = new OrderViewModel
                        {
                            Id = o.Id,
                            UserId = o.UserId,
                            OrderDate = o.OrderDate,
                            Status = o.Status,
                            TotalAmount = o.TotalAmount,
                            ShippingAddress = new ShippingAddressViewModel
                            {
                                FullName = o.ShippingAddress.FullName,
                                Phone = o.ShippingAddress.Phone,
                                AddressLine1 = o.ShippingAddress.AddressLine1,
                                AddressLine2 = o.ShippingAddress.AddressLine2,
                                City = o.ShippingAddress.City,
                                State = o.ShippingAddress.State,
                                PostalCode = o.ShippingAddress.PostalCode,
                                Country = o.ShippingAddress.Country
                            },
                            Items = o.Items.Select(i => new OrderItemViewModel
                            {
                                Id = i.Id,
                                ProductId = i.ProductId,
                                ProductName = i.ProductName,
                                Quantity = i.Quantity,
                                UnitPrice = i.UnitPrice,
                                Subtotal = i.Subtotal
                            }).ToList()
                        };
                    }
                }
                else { return NotFound(); }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load order {Id}", id);
                TempData["Error"] = "Could not load order details.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ── POST /Order/Cancel/{id} ───────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var resp = await _orderClient.DeleteAsync(
                    $"api/users/{CurrentUserId}/orders/{id}");

                TempData[resp.IsSuccessStatusCode ? "Success" : "Error"] =
                    resp.IsSuccessStatusCode
                        ? $"Order #{id} has been cancelled."
                        : "Could not cancel this order. It may already be shipped.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel order {Id}", id);
                TempData["Error"] = "Something went wrong.";
            }

            return RedirectToAction(nameof(Detail), new { id });
        }

        // ── Internal API response shapes ──────────────────────
        private class ApiWrapper<T>
        {
            public bool Success { get; set; }
            public T? Data { get; set; }
            public string Message { get; set; } = string.Empty;
        }
        private class OrderSummaryResponse
        {
            public int Id { get; set; }
            public DateTime OrderDate { get; set; }
            public string Status { get; set; } = string.Empty;
            public decimal TotalAmount { get; set; }
            public int TotalItems { get; set; }
        }
        private class OrderDetailResponse
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public DateTime OrderDate { get; set; }
            public string Status { get; set; } = string.Empty;
            public decimal TotalAmount { get; set; }
            public AddressResponse ShippingAddress { get; set; } = new();
            public List<ItemResponse> Items { get; set; } = new();
        }
        private class AddressResponse
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
        private class ItemResponse
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Subtotal { get; set; }
        }
    }

}
