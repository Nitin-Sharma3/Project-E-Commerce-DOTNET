using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Reviews.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;
        private readonly IConfiguration _config;

        // Allowed file extensions
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".webm", ".mov", ".avi" };
        private static readonly long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        public UploadController(ILogger<UploadController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "Upload endpoint. Use POST with form-data 'file'.",
                allowedFormats = new { images = "jpg, jpeg, png, gif, webp", videos = "mp4, webm, mov, avi" },
                maxFileSize = "10 MB"
            });
        }

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromQuery] string type = "image")
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file uploaded" });

                // Validate file extension
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var isAllowedFile = type.ToLowerInvariant() switch
                {
                    "image" => AllowedImageExtensions.Contains(fileExtension),
                    "video" => AllowedVideoExtensions.Contains(fileExtension),
                    _ => false
                };

                if (!isAllowedFile)
                    return BadRequest(new
                    {
                        error = $"Invalid file type. Allowed: {(type == "image" ? string.Join(", ", AllowedImageExtensions) : string.Join(", ", AllowedVideoExtensions))}"
                    });

                // Validate file size
                if (file.Length > MaxFileSize)
                    return BadRequest(new
                    {
                        error = $"File size exceeds maximum limit of {MaxFileSize / (1024 * 1024)} MB"
                    });

                // Validate MIME type
                if (!ValidateMimeType(file.ContentType, type))
                    return BadRequest(new { error = "Invalid MIME type for the file" });

                // Create upload directory
                var folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "reviews",
                    DateTime.Now.Year.ToString(),
                    DateTime.Now.Month.ToString("D2")
                );

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // Generate safe filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(folderPath, fileName);

                // Ensure the path is within wwwroot (prevent directory traversal)
                var fullPath = Path.GetFullPath(filePath);
                var wwwrootPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
                if (!fullPath.StartsWith(wwwrootPath))
                    return BadRequest(new { error = "Invalid file path" });

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative URL
                var relativePath = $"/uploads/reviews/{DateTime.Now.Year}/{DateTime.Now.Month:D2}/{fileName}";

                _logger.LogInformation($"File uploaded successfully: {fileName}");

                return Ok(new
                {
                    success = true,
                    url = relativePath,
                    fileName = fileName,
                    type = type
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, new { error = "An error occurred while uploading the file" });
            }
        }

        /// <summary>
        /// Validates MIME type against file type
        /// </summary>
        private bool ValidateMimeType(string contentType, string type)
        {
            if (string.IsNullOrEmpty(contentType))
                return false;

            var imageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            var videoTypes = new[] { "video/mp4", "video/webm", "video/quicktime", "video/x-msvideo" };

            return type.ToLowerInvariant() switch
            {
                "image" => imageTypes.Contains(contentType.ToLowerInvariant()),
                "video" => videoTypes.Contains(contentType.ToLowerInvariant()),
                _ => false
            };
        }
    }
}