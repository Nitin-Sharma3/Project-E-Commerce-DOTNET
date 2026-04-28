using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reviews.DTOs;
using Reviews.Models;
using Reviews.Repositories;
using Reviews.Services;
using System.Security.Claims;

namespace Reviews.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _repo;
        private readonly IOrderService _orderService;
        private readonly IReviewSummaryService _summaryService;
        private readonly IReviewVoteRepository _voteRepo;
        private readonly IReviewReportRepository _reportRepo;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(
            IReviewRepository repo,
            IOrderService orderService,
            IReviewSummaryService summaryService,
            IReviewVoteRepository voteRepo,
            IReviewReportRepository reportRepo,
            ILogger<ReviewController> logger)
        {
            _repo = repo;
            _orderService = orderService;
            _summaryService = summaryService;
            _voteRepo = voteRepo;
            _reportRepo = reportRepo;
            _logger = logger;
        }
        [Authorize]
        [HttpPost("{id}/like")]
        public async Task<IActionResult> Like(string id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            await _repo.LikeReview(id, userId);

            var review = await _repo.GetById(id);

            return Ok(new { likeCount = review?.LikeCount ?? 0 });
        }
        /// <summary>
        /// Add a new review - requires authentication
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var isAuthenticated = User.Identity?.IsAuthenticated == true;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userId = isAuthenticated && int.TryParse(userIdClaim, out var parsedId) ? parsedId : 0;
                var userName = isAuthenticated ? (User.Identity?.Name ?? "User") : "Guest";

                if (isAuthenticated)
                {
                    var hasPurchased = await _orderService.HasUserPurchasedProduct(userId, dto.ProductId);
                    if (!hasPurchased)
                    {
                        _logger.LogWarning($"User {userId} attempted to review unpurchased product {dto.ProductId}");
                        return BadRequest(new { error = "You can only review products you have purchased" });
                    }

                    var existing = await _repo.GetByUserAndProduct(userId, dto.ProductId);
                    if (existing != null)
                    {
                        _logger.LogWarning($"User {userId} attempted duplicate review for product {dto.ProductId}");
                        return BadRequest(new { error = "You have already reviewed this product" });
                    }
                }

                // Create review
                var review = new Review
                {
                    ProductId = dto.ProductId,
                    UserId = userId,
                    UserName = userName,
                    OrderId = dto.OrderId,
                    Rating = dto.Rating,
                    ReviewText = dto.ReviewText,
                    Media = dto.Media?.Select(x => new MediaItem
                    {
                        Type = x.Type,
                        Url = x.Url
                    }).ToList() ?? new List<MediaItem>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _repo.AddReview(review);

                // Update summary
                await _summaryService.UpdateSummary(dto.ProductId, dto.Rating, true);

                _logger.LogInformation($"Review created by user {userId} for product {dto.ProductId}");

                return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding review");
                return StatusCode(500, new { error = "An error occurred while creating the review" });
            }
        }

        /// <summary>
        /// Update an existing review - only owner can update
        /// </summary>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateReviewDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { error = "Invalid user information" });

                var review = await _repo.GetById(id);
                if (review == null)
                    return NotFound(new { error = "Review not found" });

                // Check authorization
                if (review.UserId != userId)
                {
                    _logger.LogWarning($"User {userId} attempted to update review {id} they don't own");
                    return Forbid();
                }

                var oldRating = review.Rating;

                review.Rating = dto.Rating;
                review.ReviewText = dto.ReviewText;
                review.Media = dto.Media?.Select(x => new MediaItem
                {
                    Type = x.Type,
                    Url = x.Url
                }).ToList() ?? new List<MediaItem>();

                review.IsEdited = true;
                review.EditedAt = DateTime.UtcNow;
                review.UpdatedAt = DateTime.UtcNow;

                await _repo.UpdateReview(review);

                // Update summary if rating changed
                if (oldRating != dto.Rating)
                    await _summaryService.UpdateRating(review.ProductId, oldRating, dto.Rating);

                _logger.LogInformation($"Review {id} updated by user {userId}");

                return Ok(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating review {id}");
                return StatusCode(500, new { error = "An error occurred while updating the review" });
            }
        }

        /// <summary>
        /// Get all reviews for a product
        /// </summary>
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetReviews(int productId, [FromQuery] string sort = "latest")
        {
            try
            {
                var reviews = await _repo.GetByProduct(productId, sort);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching reviews for product {productId}");
                return StatusCode(500, new { error = "An error occurred while fetching reviews" });
            }
        }

        /// <summary>
        /// Get review by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(string id)
        {
            try
            {
                var review = await _repo.GetById(id);
                if (review == null)
                    return NotFound(new { error = "Review not found" });

                return Ok(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching review {id}");
                return StatusCode(500, new { error = "An error occurred while fetching the review" });
            }
        }

        /// <summary>
        /// Get review summary for a product
        /// </summary>
        [HttpGet("summary/{productId}")]
        public async Task<IActionResult> GetSummary(int productId)
        {
            try
            {
                var summary = await _summaryService.GetSummary(productId);
                if (summary == null)
                    return NotFound(new { error = "No reviews found for this product" });

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching summary for product {productId}");
                return StatusCode(500, new { error = "An error occurred while fetching the summary" });
            }
        }

        /// <summary>
        /// Vote on review helpfulness - requires authentication
        /// </summary>
        [Authorize]
        [HttpPost("{id}/vote")]
        public async Task<IActionResult> Vote(string id, [FromBody] VoteRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { error = "Invalid user information" });

                // Check if already voted
                var alreadyVoted = await _voteRepo.HasVoted(id, userId);
                if (alreadyVoted)
                {
                    _logger.LogWarning($"User {userId} attempted duplicate vote on review {id}");
                    return BadRequest(new { error = "You have already voted on this review" });
                }

                await _voteRepo.AddVote(new ReviewVote
                {
                    ReviewId = id,
                    UserId = userId,
                    IsHelpful = request.IsHelpful
                });

                var review = await _repo.GetById(id);
                if (review == null)
                    return NotFound(new { error = "Review not found" });

                if (request.IsHelpful)
                    review.HelpfulCount++;
                else
                    review.NotHelpfulCount++;

                await _repo.UpdateReview(review);

                _logger.LogInformation($"Vote recorded on review {id} by user {userId}");

                return Ok(new { message = "Vote recorded successfully", helpful = review.HelpfulCount, notHelpful = review.NotHelpfulCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error voting on review {id}");
                return StatusCode(500, new { error = "An error occurred while recording your vote" });
            }
        }

        /// <summary>
        /// Report a review - requires authentication
        /// </summary>
        [Authorize]
        [HttpPost("{id}/report")]
        public async Task<IActionResult> Report(string id, [FromBody] ReportRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { error = "Invalid user information" });

                if (string.IsNullOrWhiteSpace(request.Reason))
                    return BadRequest(new { error = "Reason is required" });

                // Check if already reported
                var alreadyReported = await _reportRepo.HasReported(id, userId);
                if (alreadyReported)
                {
                    _logger.LogWarning($"User {userId} attempted duplicate report on review {id}");
                    return BadRequest(new { error = "You have already reported this review" });
                }

                await _reportRepo.AddReport(new ReviewReport
                {
                    ReviewId = id,
                    UserId = userId,
                    Reason = request.Reason
                });

                var review = await _repo.GetById(id);
                if (review == null)
                    return NotFound(new { error = "Review not found" });

                review.ReportCount++;
                await _repo.UpdateReview(review);

                _logger.LogInformation($"Report submitted for review {id} by user {userId}");

                return Ok(new { message = "Report submitted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reporting review {id}");
                return StatusCode(500, new { error = "An error occurred while submitting your report" });
            }
        }
    }

    // Request models for binding
    public class VoteRequest
    {
        public bool IsHelpful { get; set; }
    }

    public class ReportRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}