using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Reviews.Models;
using Reviews.MongoDBSettings;

namespace Reviews.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IMongoCollection<Review> _collection;

        public ReviewRepository(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var db = client.GetDatabase(settings.Value.DatabaseName);

            _collection = db.GetCollection<Review>(settings.Value.ReviewCollection);

            var indexKeys = Builders<Review>.IndexKeys
                .Ascending(x => x.ProductId)
                .Ascending(x => x.UserId);

            _collection.Indexes.CreateOne(new CreateIndexModel<Review>(indexKeys));
        }

        public async Task AddReview(Review review)
        {
            await _collection.InsertOneAsync(review);
        }

        public async Task<Review?> GetByUserAndProduct(int userId, int productId)
        {
            return await _collection
                .Find(x => x.UserId == userId && x.ProductId == productId)
                .FirstOrDefaultAsync();
        }

        public async Task<Review?> GetById(string id)
        {
            return await _collection
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Review>> GetByProduct(int productId, string sort = "latest")
        {
            var query = _collection.Find(x => x.ProductId == productId);

            return sort switch
            {
                "highest" => await query.SortByDescending(x => x.Rating).ToListAsync(),
                "lowest" => await query.SortBy(x => x.Rating).ToListAsync(),
                "helpful" => await query.SortByDescending(x => x.HelpfulCount).ToListAsync(),
                _ => await query.SortByDescending(x => x.CreatedAt).ToListAsync()
            };
        }

        public async Task UpdateReview(Review review)
        {
            await _collection.ReplaceOneAsync(x => x.Id == review.Id, review);
        }

        public async Task LikeReview(string reviewId, int userId)
        {
            var review = await _collection
                .Find(x => x.Id == reviewId)
                .FirstOrDefaultAsync();

            if (review == null)
                return;

            // prevent duplicate likes
            if (review.LikedByUsers.Contains(userId))
                return;

            review.LikedByUsers.Add(userId);
            review.LikeCount++;

            await _collection.ReplaceOneAsync(x => x.Id == reviewId, review);
        }

    }
}