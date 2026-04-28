using Reviews.Models;

namespace Reviews.Repositories
{
    public interface IReviewRepository
    {
        Task AddReview(Review review);
        Task<Review> GetByUserAndProduct(int userId, int productId);
        Task<Review?> GetById(string id);
        Task<List<Review>> GetByProduct(int productId, string sort);
        Task UpdateReview(Review review);
        Task LikeReview(string reviewId, int userId);
    }
}
