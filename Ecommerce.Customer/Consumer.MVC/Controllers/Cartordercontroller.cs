using Microsoft.AspNetCore.Mvc;
  using Consumer.MVC.DTOs;
    using Consumer.MVC.ViewModel;
   
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
namespace Consumer.MVC.Controllers
{
  

        public class CartController : Controller
        {
            private readonly IHttpClientFactory _httpClientFactory;
            private readonly ILogger<CartController> _logger;
            private const string ApiBase = "api/Cart";

            public CartController(IHttpClientFactory httpClientFactory, ILogger<CartController> logger)
            {
                _httpClientFactory = httpClientFactory;
                _logger = logger;
            }

            // GET /Cart
            public async Task<IActionResult> Index()
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("OrderApi");
                    var response = await client.GetAsync(ApiBase);

                    if (!response.IsSuccessStatusCode)
                        return View(new CartViewModel());

                    var apiData = await response.Content.ReadFromJsonAsync<CartApiResponse>();
                    var vm = MapToViewModel(apiData);
                    return View(vm);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load cart");
                    TempData["Error"] = "Unable to load your cart. Please try again.";
                    return View(new CartViewModel());
                }
            }

            // POST /Cart/Add
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Add([FromBody] AddToCartDto dto)
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid input." });

                try
                {
                    var client = _httpClientFactory.CreateClient("OrderApi");
                    var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{ApiBase}/add", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var cartData = await response.Content.ReadFromJsonAsync<CartApiResponse>();
                        return Json(new { success = true, itemCount = cartData?.Items?.Sum(i => i.Quantity) ?? 0 });
                    }

                    return Json(new { success = false, message = "Could not add item to cart." });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Add to cart failed for product {ProductId}", dto.ProductId);
                    return Json(new { success = false, message = "Something went wrong." });
                }
            }

            // POST /Cart/Update
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Update([FromBody] UpdateCartDto dto)
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid input." });

                try
                {
                    var client = _httpClientFactory.CreateClient("OrderApi");
                    var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                    var response = await client.PutAsync($"{ApiBase}/update", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var cartData = await response.Content.ReadFromJsonAsync<CartApiResponse>();
                        var vm = MapToViewModel(cartData);
                        return Json(new
                        {
                            success = true,
                            lineTotal = vm.Items.FirstOrDefault(i => i.ProductId == dto.ProductId)?.LineTotal.ToString("C"),
                            subtotal = vm.Subtotal.ToString("C"),
                            total = vm.Total.ToString("C"),
                            itemCount = vm.ItemCount
                        });
                    }

                    return Json(new { success = false, message = "Could not update cart." });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Update cart failed");
                    return Json(new { success = false, message = "Something went wrong." });
                }
            }

            // POST /Cart/Remove/{productId}
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Remove(int productId)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("OrderApi");
                    var response = await client.DeleteAsync($"{ApiBase}/remove/{productId}");

                    if (response.IsSuccessStatusCode)
                        return Json(new { success = true });

                    return Json(new { success = false, message = "Could not remove item." });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Remove from cart failed");
                    return Json(new { success = false, message = "Something went wrong." });
                }
            }

            // POST /Cart/Clear
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Clear()
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("OrderApi");
                    await client.DeleteAsync($"{ApiBase}/clear");
                    TempData["Success"] = "Cart cleared.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Clear cart failed");
                    TempData["Error"] = "Could not clear cart.";
                }

                return RedirectToAction(nameof(Index));
            }

            // ── Private mapper ─────────────────────────────────────────────────────
            private static CartViewModel MapToViewModel(CartApiResponse dto)
            {
                if (dto == null) return new CartViewModel();
                return new CartViewModel
                {
                    Items = dto.Items?.Select(i => new CartItemViewModel
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ProductImage = i.ProductImage ?? "/images/placeholder.webp",
                        Category = i.Category,
                        Brand = i.Brand,
                        UnitPrice = i.UnitPrice,
                        DiscountedPrice = i.DiscountedPrice,
                        Quantity = i.Quantity,
                        InStock = i.InStock,
                        MaxQuantity = i.MaxQuantity,
                        Sku = i.Sku
                    }).ToList() ?? new(),
                    Subtotal = dto.Subtotal,
                    Tax = dto.Tax,
                    Shipping = dto.ShippingCost,
                    Discount = dto.Discount,
                    Total = dto.Total,
                    ItemCount = dto.Items?.Sum(i => i.Quantity) ?? 0
                };
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  Order Controller
        // ══════════════════════════════════════════════════════════════════════════
     
        public class OrderController : Controller
        {
            private readonly IHttpClientFactory _httpClientFactory;
            private readonly ILogger<OrderController> _logger;
            private const string ApiBase = "api/Order";
            private const string AddressApi = "api/Address";

            public OrderController(IHttpClientFactory httpClientFactory, ILogger<OrderController> logger)
            {
                _httpClientFactory = httpClientFactory;
                _logger = logger;
            }

            // GET /Order
            public async Task<IActionResult> Index(string status = null, int page = 1)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("OrderApi");
                    var url = string.IsNullOrEmpty(status) ? ApiBase : $"{ApiBase}?status={status}";
                    var orders = await client.GetFromJsonAsync<System.Collections.Generic.List<OrderApiResponse>>(url)
                                 ?? new();

                    var vm = new OrderListViewModel
                    {
                        Orders = orders.Select(MapToSummary).ToList(),
                        TotalOrders = orders.Count,
                        CurrentPage = page,
                        TotalPages = (int)Math.Ceiling(orders.Count / 10.0),
                        StatusFilter = status
                    };

                    return View(vm);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load orders");
                    return View(new OrderListViewModel());
                }
            }

            // GET /Order/{id}
            public async Task<IActionResult> Detail(int id)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("OrderApi");
                    var order = await client.GetFromJsonAsync<OrderApiResponse>($"{ApiBase}/{id}");

                    if (order == null) return NotFound();

                    return View(MapToDetail(order));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load order {Id}", id);
                    return RedirectToAction(nameof(Index));
                }
            }

            // GET /Order/Checkout
            public async Task<IActionResult> Checkout()
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("OrderApi");
                    var cartTask = client.GetFromJsonAsync<CartApiResponse>("api/Cart");
                    var addressTask = client.GetFromJsonAsync<System.Collections.Generic.List<InlineAddressDto>>(AddressApi);

                    await Task.WhenAll(cartTask, addressTask);

                    var cart = CartController_MapToViewModel(cartTask.Result);
                    var addresses = addressTask.Result ?? new();

                    if (!cart.Items.Any())
                    {
                        TempData["Error"] = "Your cart is empty.";
                        return RedirectToAction("Index", "Cart");
                    }

                    var vm = new PlaceOrderViewModel
                    {
                        Cart = cart,
                        SavedAddresses = addresses.Select(a => new AddressViewModel
                        {
                            FullName = a.FullName,
                            AddressLine1 = a.Line1,
                            AddressLine2 = a.Line2,
                            City = a.City,
                            State = a.State,
                            PostalCode = a.PostalCode,
                            Country = a.Country,
                            PhoneNumber = a.Phone
                        }).ToList()
                    };

                    return View(vm);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Checkout load failed");
                    return RedirectToAction("Index", "Cart");
                }
            }

            // POST /Order/PlaceOrder
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> PlaceOrder(PlaceOrderViewModel model)
            {
                if (!ModelState.IsValid)
                    return RedirectToAction(nameof(Checkout));

                try
                {
                    var dto = new PlaceOrderDto
                    {
                        AddressId = model.AddressId,
                        PaymentMethod = model.PaymentMethod,
                        Notes = model.Notes
                    };

                    var client = _httpClientFactory.CreateClient("OrderApi");
                    var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(ApiBase, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var order = await response.Content.ReadFromJsonAsync<OrderApiResponse>();
                        TempData["OrderNumber"] = order?.OrderNumber;
                        return RedirectToAction(nameof(Confirmation), new { id = order?.Id });
                    }

                    TempData["Error"] = "Could not place your order. Please try again.";
                    return RedirectToAction(nameof(Checkout));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Place order failed");
                    TempData["Error"] = "Something went wrong placing your order.";
                    return RedirectToAction(nameof(Checkout));
                }
            }

            // GET /Order/Confirmation/{id}
            public async Task<IActionResult> Confirmation(int id)
            {
                var client = _httpClientFactory.CreateClient("OrderApi");
                var order = await client.GetFromJsonAsync<OrderApiResponse>($"{ApiBase}/{id}");
                if (order == null) return RedirectToAction(nameof(Index));
                return View(MapToDetail(order));
            }

            // POST /Order/{id}/Cancel
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Cancel(int id)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("OrderApi");
                    var response = await client.DeleteAsync($"{ApiBase}/{id}/cancel");

                    TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                        response.IsSuccessStatusCode ? "Order cancelled successfully." : "Could not cancel order.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cancel order failed");
                    TempData["Error"] = "Something went wrong.";
                }

                return RedirectToAction(nameof(Detail), new { id });
            }

            // ── Private mappers ────────────────────────────────────────────────────
            private static string GetStatusColor(string status) => status?.ToLower() switch
            {
                "pending" => "#F59E0B",
                "confirmed" => "#3B82F6",
                "processing" => "#8B5CF6",
                "shipped" => "#06B6D4",
                "delivered" => "#10B981",
                "cancelled" => "#EF4444",
                "refunded" => "#6B7280",
                _ => "#212121"
            };

            private static string GetTimelineIcon(string status) => status?.ToLower() switch
            {
                "pending" => "clock",
                "confirmed" => "check-circle",
                "processing" => "settings",
                "shipped" => "truck",
                "delivered" => "package",
                "cancelled" => "x-circle",
                _ => "circle"
            };

            private static OrderSummaryViewModel MapToSummary(OrderApiResponse o) => new()
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                PlacedAt = o.PlacedAt,
                Status = o.Status,
                StatusColor = GetStatusColor(o.Status),
                Total = o.Total,
                ItemCount = o.Items?.Count ?? 0,
                FirstProductName = o.Items?.FirstOrDefault()?.ProductName,
                ThumbnailUrl = o.Items?.FirstOrDefault()?.ProductImage ?? "/images/placeholder.webp"
            };

            private static OrderDetailViewModel MapToDetail(OrderApiResponse o)
            {
                var statuses = new[] { "Pending", "Confirmed", "Processing", "Shipped", "Delivered" };
                var currentIdx = Array.IndexOf(statuses, o.Status);

                return new OrderDetailViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    PlacedAt = o.PlacedAt,
                    Status = o.Status,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,
                    Items = o.Items?.Select(i => new OrderItemViewModel
                    {
                        ProductName = i.ProductName,
                        ProductImage = i.ProductImage ?? "/images/placeholder.webp",
                        Category = i.Category,
                        Sku = i.Sku,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        LineTotal = i.LineTotal
                    }).ToList() ?? new(),
                    ShippingAddress = o.ShippingAddress == null ? null : new AddressViewModel
                    {
                        FullName = o.ShippingAddress.FullName,
                       AddressLine1 = o.ShippingAddress.Line1,
                        AddressLine2 = o.ShippingAddress.Line2,
                        City = o.ShippingAddress.City,
                        State = o.ShippingAddress.State,
                        PostalCode = o.ShippingAddress.PostalCode,
                        Country = o.ShippingAddress.Country,
                        PhoneNumber = o.ShippingAddress.Phone
                    },
                    Subtotal = o.Subtotal,
                    Tax = o.Tax,
                    ShippingCost = o.ShippingCost,
                    Discount = o.Discount,
                    Total = o.Total,
                    TrackingNumber = o.TrackingNumber,
                    CanCancel = o.Status is "Pending" or "Confirmed",
                    Timeline = statuses.Select((s, idx) => new OrderTimelineEvent
                    {
                        Status = s,
                        Description = GetTimelineDescription(s),
                        Timestamp = o.Timeline?.FirstOrDefault(t => t.Status == s)?.Timestamp ?? default,
                        IsCompleted = idx < currentIdx,
                        IsCurrent = idx == currentIdx,
                        Icon = GetTimelineIcon(s)
                    }).ToList()
                };
            }

            private static string GetTimelineDescription(string status) => status switch
            {
                "Pending" => "Order placed and awaiting confirmation",
                "Confirmed" => "Seller confirmed your order",
                "Processing" => "Your items are being packed",
                "Shipped" => "Order handed to courier",
                "Delivered" => "Order delivered successfully",
                _ => status
            };

            // Reuse cart mapper without circular dependency
            private static CartViewModel CartController_MapToViewModel(CartApiResponse dto)
            {
                if (dto == null) return new CartViewModel();
                return new CartViewModel
                {
                    Items = dto.Items?.Select(i => new CartItemViewModel
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ProductImage = i.ProductImage ?? "/images/placeholder.webp",
                        Category = i.Category,
                        Brand = i.Brand,
                        UnitPrice = i.UnitPrice,
                        DiscountedPrice = i.DiscountedPrice,
                        Quantity = i.Quantity,
                        InStock = i.InStock,
                        MaxQuantity = i.MaxQuantity,
                        Sku = i.Sku
                    }).ToList() ?? new(),
                    Subtotal = dto.Subtotal,
                    Tax = dto.Tax,
                    Shipping = dto.ShippingCost,
                    Discount = dto.Discount,
                    Total = dto.Total,
                    ItemCount = dto.Items?.Sum(i => i.Quantity) ?? 0
                };
            }
        }
    }