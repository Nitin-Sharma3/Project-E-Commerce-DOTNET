using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Reviews.Models
{
    public class ReviewVote
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ReviewId { get; set; }
        public int UserId { get; set; }
        public bool IsHelpful { get; set; }
    }
}
