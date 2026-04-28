using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Reviews.Models;
using Reviews.MongoDBSettings;

namespace Reviews.Repositories
{
    public class ReviewVoteRepository : IReviewVoteRepository
    {
        private readonly IMongoCollection<ReviewVote> _collection;

        public ReviewVoteRepository(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var db = client.GetDatabase(settings.Value.DatabaseName);

            _collection = db.GetCollection<ReviewVote>(settings.Value.VoteCollection);

            // prevent duplicate voting (same user + same review)
            var indexKeys = Builders<ReviewVote>.IndexKeys
                .Ascending(x => x.ReviewId)
                .Ascending(x => x.UserId);

            _collection.Indexes.CreateOne(
                new CreateIndexModel<ReviewVote>(indexKeys, new CreateIndexOptions { Unique = true })
            );
        }

        public async Task<bool> HasVoted(string reviewId, int userId)
        {
            return await _collection
                .Find(x => x.ReviewId == reviewId && x.UserId == userId)
                .AnyAsync();
        }

        public async Task AddVote(ReviewVote vote)
        {
            await _collection.InsertOneAsync(vote);
        }
    }
}
