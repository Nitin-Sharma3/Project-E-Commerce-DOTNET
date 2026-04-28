using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Reviews.Models
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public int ProductId { get; set; }
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public int OrderId { get; set; }

        public int Rating { get; set; } // will validate in DTO/service

        public string ReviewText { get; set; } = string.Empty;

        public List<MediaItem> Media { get; set; } = new();

        public bool IsVerifiedPurchase { get; set; } = true;

        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; }

        public int HelpfulCount { get; set; } = 0;
        public int NotHelpfulCount { get; set; } = 0;

        public int ReportCount { get; set; } = 0;

        public string? Sentiment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int LikeCount { get; set; } = 0;
        public List<int> LikedByUsers { get; set; } = new();
    }

    public class MediaItem
    {
        public string Type { get; set; } = string.Empty; // image/video
        public string Url { get; set; } = string.Empty;
    }
}