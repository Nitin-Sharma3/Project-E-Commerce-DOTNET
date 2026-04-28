using Reviews.Models;

namespace Reviews.Repositories
{
    public interface IReviewReportRepository
    {
        Task AddReport(ReviewReport report);
        Task<bool> HasReported(string reviewId, int userId);
    }
}
