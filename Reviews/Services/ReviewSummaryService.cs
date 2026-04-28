using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Reviews.Models;
using Reviews.MongoDBSettings;

namespace Reviews.Services
{
    public class ReviewSummaryService : IReviewSummaryService
    {
        private readonly IMongoCollection<ReviewSummary> _collection;

        public ReviewSummaryService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var db = client.GetDatabase(settings.Value.DatabaseName);

            _collection = db.GetCollection<ReviewSummary>(settings.Value.SummaryCollection);
        }

        public async Task UpdateSummary(int productId, int rating, bool isNew)
        {
            var summary = await _collection.Find(x => x.ProductId == productId).FirstOrDefaultAsync();

            if (summary == null)
            {
                summary = new ReviewSummary
                {
                    ProductId = productId,
                    TotalReviews = 1,
                    AverageRating = rating,
                    RatingDistribution = new Dictionary<string, int>
                    {
                        {"1",0},{"2",0},{"3",0},{"4",0},{"5",0}
                    }
                };

                summary.RatingDistribution[rating.ToString()] = 1;

                await _collection.InsertOneAsync(summary);
                return;
            }

            if (isNew)
            {
                summary.TotalReviews += 1;
                summary.RatingDistribution[rating.ToString()]++;
            }

            summary.AverageRating =
                ((summary.AverageRating * (summary.TotalReviews - 1)) + rating)
                / summary.TotalReviews;

            await _collection.ReplaceOneAsync(x => x.Id == summary.Id, summary);
        }

        public async Task UpdateRating(int productId, int oldRating, int newRating)
        {
            var summary = await _collection.Find(x => x.ProductId == productId).FirstOrDefaultAsync();

            if (summary == null) return;

            // 🔥 FIX: use string keys
            summary.RatingDistribution[oldRating.ToString()]--;
            summary.RatingDistribution[newRating.ToString()]++;

            // 🔥 FIX: convert string → int
            var totalScore = summary.RatingDistribution
                .Sum(x => int.Parse(x.Key) * x.Value);

            summary.AverageRating =
                (double)totalScore / summary.TotalReviews;

            await _collection.ReplaceOneAsync(x => x.Id == summary.Id, summary);
        }

        public async Task<ReviewSummary?> GetSummary(int productId)
        {
            return await _collection
                .Find(x => x.ProductId == productId)
                .FirstOrDefaultAsync();
        }
    }
}