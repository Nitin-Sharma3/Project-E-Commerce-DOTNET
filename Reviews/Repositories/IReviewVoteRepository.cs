using Reviews.Models;

namespace Reviews.Repositories
{
    public interface IReviewVoteRepository
    {
        Task<bool> HasVoted(string reviewId, int userId);
        Task AddVote(ReviewVote vote);
    }
}
