using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

using UserDashboardMVC.Models;

namespace UserDashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;

        private string ProductApiBase =>
            _configuration["ProductApi:BaseUrl"] ?? "https://localhost:7001/api/";

        public HomeController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<HomeController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        // ─── GET / ────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var vm = new HomeIndexViewModel();

            try
            {
                var client = BuildApiClient();

                _logger.LogInformation("Calling Product API at: {Url}", ProductApiBase + "Product");

                var resp = await client.GetAsync("Product");

                _logger.LogInformation("Product API response: {StatusCode}", resp.StatusCode);

                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    _logger.LogInformation("Product API raw JSON (first 300 chars): {Json}",
                        json.Length > 300 ? json[..300] : json);

                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    vm.Products = JsonSerializer.Deserialize<List<ProductDto>>(json, opts) ?? new();

                    _logger.LogInformation("Deserialized {Count} products", vm.Products.Count);

                    vm.Categories = vm.Products
                        .Where(p => !string.IsNullOrWhiteSpace(p.Category))
                        .Select(p => p.Category!)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(c => c)
                        .ToList();
                }
                else
                {
                    var errorBody = await resp.Content.ReadAsStringAsync();
                    _logger.LogWarning("Product API error {StatusCode}: {Body}", resp.StatusCode, errorBody);
                    vm.ProductApiError = true;
                    // Pass the error detail to the view via TempData so you can see it on screen
                    ViewBag.ApiErrorDetail = $"HTTP {(int)resp.StatusCode} {resp.StatusCode} — {errorBody}";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HttpRequestException reaching Product API at {Url}", ProductApiBase);
                vm.ProductApiError = true;
                ViewBag.ApiErrorDetail = $"Cannot reach API at '{ProductApiBase}' — {ex.Message}";
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialisation failed");
                vm.ProductApiError = true;
                ViewBag.ApiErrorDetail = $"JSON parse error — {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching products");
                vm.ProductApiError = true;
                ViewBag.ApiErrorDetail = $"Unexpected error — {ex.GetType().Name}: {ex.Message}";
            }

            return View(vm);
        }

        // ─── GET /Home/Debug ── shows config & a raw API ping ────────────────────
        // Remove this action once products are loading correctly
        [HttpGet]
        public async Task<IActionResult> Debug()
        {
            var result = new System.Text.StringBuilder();
            result.AppendLine($"ProductApiBase config value : {ProductApiBase}");
            result.AppendLine($"Calling URL                 : {ProductApiBase}Product");
            result.AppendLine();

            try
            {
                var client = BuildApiClient();
                var resp = await client.GetAsync("Product");
                var body = await resp.Content.ReadAsStringAsync();
                result.AppendLine($"HTTP Status : {(int)resp.StatusCode} {resp.StatusCode}");
                result.AppendLine($"Response    : {(body.Length > 800 ? body[..800] + "…" : body)}");
            }
            catch (Exception ex)
            {
                result.AppendLine($"EXCEPTION   : {ex.GetType().Name}");
                result.AppendLine($"Message     : {ex.Message}");
                if (ex.InnerException != null)
                    result.AppendLine($"Inner       : {ex.InnerException.Message}");
            }

            return Content(result.ToString(), "text/plain");
        }

        // ─── GET /Home/ProductsByCategory?category=Glass+Covers ──────────────────
        [HttpGet]
        public async Task<IActionResult> ProductsByCategory(string category)
        {
            try
            {
                var client = BuildApiClient();
                var resp = await client.GetAsync($"Product?category={Uri.EscapeDataString(category)}");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var products = JsonSerializer.Deserialize<List<ProductDto>>(json, opts) ?? new();
                    return Json(new { success = true, data = products });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products for category {Category}", category);
            }
            return Json(new { success = false, data = Array.Empty<object>() });
        }

        // ─── GET /Home/ProductDetail/{id} ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ProductDetail(int id)
        {
            try
            {
                var client = BuildApiClient();
                var resp = await client.GetAsync($"Product/{id}");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var product = JsonSerializer.Deserialize<ProductDto>(json, opts);
                    if (product != null)
                        return Json(new { success = true, data = product });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product {Id}", id);
            }
            return Json(new { success = false });
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────
        private HttpClient BuildApiClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(ProductApiBase);
            client.DefaultRequestHeaders.Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}
