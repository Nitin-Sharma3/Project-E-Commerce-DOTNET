using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reviews.DTOs;
using Reviews.Models;
using System.Net.Http;
using System.Text.Json;

namespace Reviews.Pages.Reviews
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EditModel> _logger;

        [BindProperty]
        public Review Review { get; set; } = new();

        [BindProperty]
        public UpdateReviewDTO UpdateReview { get; set; } = new();

        [BindProperty]
        public List<IFormFile>? MediaFiles { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public EditModel(HttpClient httpClient, ILogger<EditModel> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/review/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Review not found";
                    return RedirectToPage("Index");
                }

                var content = await response.Content.ReadAsStringAsync();
                Review = JsonSerializer.Deserialize<Review>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new();

                // Populate update DTO with current values
                UpdateReview.Rating = Review.Rating;
                UpdateReview.ReviewText = Review.ReviewText;
                UpdateReview.Media = Review.Media?.Select(x => new MediaItemDTO
                {
                    Type = x.Type,
                    Url = x.Url
                }).ToList() ?? new List<MediaItemDTO>();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading review {id}");
                ErrorMessage = "Failed to load review";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ErrorMessage = "Please fill in all required fields correctly";
                    return await OnGetAsync(id);
                }

                // Upload new media files if provided
                if (MediaFiles != null && MediaFiles.Count > 0)
                {
                    foreach (var file in MediaFiles)
                    {
                        try
                        {
                            var fileType = DetermineFileType(file.ContentType);
                            var uploadUrl = await UploadMediaFile(file, fileType);

                            UpdateReview.Media.Add(new MediaItemDTO
                            {
                                Type = fileType,
                                Url = uploadUrl
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error uploading file: {file.FileName}");
                            ErrorMessage = $"Failed to upload {file.FileName}";
                            return await OnGetAsync(id);
                        }
                    }
                }

                // Send update to API
                var token = Request.Cookies["jwt_token"];
                if (string.IsNullOrEmpty(token))
                {
                    ErrorMessage = "Authentication token not found. Please log in again.";
                    return RedirectToPage("/Login");
                }

                var request = new HttpRequestMessage(HttpMethod.Put, $"/api/review/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Content = new StringContent(
                    JsonSerializer.Serialize(UpdateReview),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to update review: {response.StatusCode}";
                    _logger.LogError($"API Error: {errorContent}");
                    return await OnGetAsync(id);
                }

                SuccessMessage = "Review updated successfully!";
                return RedirectToPage("Index", new { productId = Review.ProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review");
                ErrorMessage = "An error occurred while updating your review";
                return await OnGetAsync(id);
            }
        }

        private async Task<string> UploadMediaFile(IFormFile file, string fileType)
        {
            var form = new MultipartFormDataContent();
            var fileStream = file.OpenReadStream();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            form.Add(streamContent, "file", file.FileName);

            var response = await _httpClient.PostAsync($"/api/upload?type={fileType}", form);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseContent);
            var url = doc.RootElement.GetProperty("url").GetString();

            return url ?? throw new Exception("No URL in upload response");
        }

        private string DetermineFileType(string? contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return "image";

            return contentType.StartsWith("video") ? "video" : "image";
        }

        public string GetStarDisplay(int rating)
        {
            return string.Concat(Enumerable.Range(0, 5).Select(i => i < rating ? "★" : "☆"));
        }
    }
}