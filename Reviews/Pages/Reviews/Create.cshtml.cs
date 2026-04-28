using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reviews.DTOs;
using System.Net.Http;
using System.Text.Json;

namespace Reviews.Pages.Reviews
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        public CreateReviewDTO Review { get; set; } = new();

        [BindProperty]
        public List<IFormFile>? MediaFiles { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public CreateModel(HttpClient httpClient, ILogger<CreateModel> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void OnGet(int productId, int orderId)
        {
            Review.ProductId = productId;
            Review.OrderId = orderId;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    ErrorMessage = "Please fill in all required fields correctly";
                    return Page();
                }

                // Upload media files if provided
                if (MediaFiles != null && MediaFiles.Count > 0)
                {
                    Review.Media = new List<MediaItemDTO>();

                    foreach (var file in MediaFiles)
                    {
                        try
                        {
                            var fileType = DetermineFileType(file.ContentType);
                            var uploadUrl = await UploadMediaFile(file, fileType);

                            Review.Media.Add(new MediaItemDTO
                            {
                                Type = fileType,
                                Url = uploadUrl
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error uploading file: {file.FileName}");
                            ErrorMessage = $"Failed to upload {file.FileName}";
                            return Page();
                        }
                    }
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/review");
                request.Content = new StringContent(
                    JsonSerializer.Serialize(Review),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to submit review: {response.StatusCode}";
                    _logger.LogError($"API Error: {errorContent}");
                    return Page();
                }

                SuccessMessage = "Review submitted successfully!";
                return RedirectToPage("/Reviews/Index", new { productId = Review.ProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                ErrorMessage = "An error occurred while submitting your review";
                return Page();
            }
        }

        private async Task<string> UploadMediaFile(IFormFile file, string fileType)
        {
            var form = new MultipartFormDataContent();
            var fileStream = file.OpenReadStream();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            form.Add(streamContent, "file", file.FileName);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var response = await _httpClient.PostAsync($"{baseUrl}/api/upload?type={fileType}", form);
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
    }
}