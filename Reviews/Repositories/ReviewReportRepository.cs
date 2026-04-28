using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Reviews.Models;
using Reviews.MongoDBSettings;

namespace Reviews.Repositories
{
    public class ReviewReportRepository : IReviewReportRepository
    {
        private readonly IMongoCollection<ReviewReport> _collection;

        public ReviewReportRepository(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var db = client.GetDatabase(settings.Value.DatabaseName);

            _collection = db.GetCollection<ReviewReport>(settings.Value.ReportCollection);

            // prevent duplicate reports
            var indexKeys = Builders<ReviewReport>.IndexKeys
                .Ascending(x => x.ReviewId)
                .Ascending(x => x.UserId);

            _collection.Indexes.CreateOne(
                new CreateIndexModel<ReviewReport>(indexKeys, new CreateIndexOptions { Unique = true })
            );
        }

        public async Task AddReport(ReviewReport report)
        {
            await _collection.InsertOneAsync(report);
        }

        public async Task<bool> HasReported(string reviewId, int userId)
        {
            return await _collection
                .Find(x => x.ReviewId == reviewId && x.UserId == userId)
                .AnyAsync();
        }
    }
}
