using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Reviews.Models
{
    public class ReviewSummary
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public int ProductId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        public Dictionary<string, int> RatingDistribution { get; set; } = new()
        {
            {"1",0},{"2",0},{"3",0},{"4",0},{"5",0}
        };
    }
}