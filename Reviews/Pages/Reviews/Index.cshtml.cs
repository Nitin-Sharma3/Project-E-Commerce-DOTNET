using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reviews.Models;
using System.Text.Json;

namespace Reviews.Pages.Reviews
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IndexModel> _logger;

        public List<Review> Reviews { get; set; } = new();
        public ReviewSummary? Summary { get; set; }

        public int ProductId { get; set; }
        public string SortBy { get; set; } = "latest";
        public int? RatingFilter { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 5;

        public IndexModel(HttpClient httpClient, ILogger<IndexModel> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task OnGetAsync(int productId, string sort = "latest", int page = 1, int? rating = null)
        {
            try
            {
                ProductId = productId;
                SortBy = sort;
                CurrentPage = page;
                RatingFilter = rating;

                if (productId <= 0)
                {
                    Reviews = new();
                    Summary = null;
                    return;
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                var reviewResponse = await _httpClient.GetAsync(
                    $"{baseUrl}/api/review/product/{productId}?sort={sort}");

                if (reviewResponse.IsSuccessStatusCode)
                {
                    var content = await reviewResponse.Content.ReadAsStringAsync();

                    var allReviews = JsonSerializer.Deserialize<List<Review>>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    ) ?? new();

                    if (rating.HasValue)
                    {
                        allReviews = allReviews.Where(r => r.Rating == rating.Value).ToList();
                    }

                    allReviews = sort switch
                    {
                        "highest" => allReviews.OrderByDescending(r => r.Rating).ToList(),
                        "lowest" => allReviews.OrderBy(r => r.Rating).ToList(),
                        "helpful" => allReviews.OrderByDescending(r => r.HelpfulCount).ToList(),
                        _ => allReviews.OrderByDescending(r => r.CreatedAt).ToList()
                    };

                    Reviews = allReviews
                        .Skip((page - 1) * PageSize)
                        .Take(PageSize)
                        .ToList();
                }

                var summaryResponse = await _httpClient.GetAsync(
                    $"{baseUrl}/api/review/summary/{productId}");

                if (summaryResponse.IsSuccessStatusCode)
                {
                    var content = await summaryResponse.Content.ReadAsStringAsync();

                    Summary = JsonSerializer.Deserialize<ReviewSummary>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading reviews for product {productId}");
            }
        }

        //public async Task<IActionResult> OnPostVoteAsync(string reviewId, bool isHelpful, int productId)
        //{
        //    var baseUrl = $"{Request.Scheme}://{Request.Host}";

        //    await _httpClient.PostAsync(
        //        $"{baseUrl}/api/review/{reviewId}/vote?isHelpful={isHelpful}",
        //        null);

        //    return RedirectToPage(new { productId });
        //}

        //public async Task<IActionResult> OnPostReportAsync(string reviewId, int productId)
        //{
        //    var baseUrl = $"{Request.Scheme}://{Request.Host}";

        //    var content = new FormUrlEncodedContent(new[]
        //    {
        //        new KeyValuePair<string, string>("reason", "Inappropriate")
        //    });

        //    await _httpClient.PostAsync(
        //        $"{baseUrl}/api/review/{reviewId}/report",
        //        content);

        //    return RedirectToPage(new { productId });
        //}
    }
}