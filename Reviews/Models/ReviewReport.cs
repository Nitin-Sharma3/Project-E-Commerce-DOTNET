using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Reviews.Models
{
    public class ReviewReport
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ReviewId { get; set; }
        public int UserId { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
