using Reviews.Models;

namespace Reviews.Services
{
    public interface IReviewSummaryService
    {
        Task UpdateSummary(int productId, int rating, bool isNew);
        Task UpdateRating(int productId, int oldRating, int newRating);
        Task<ReviewSummary?> GetSummary(int productId);
    }
}
