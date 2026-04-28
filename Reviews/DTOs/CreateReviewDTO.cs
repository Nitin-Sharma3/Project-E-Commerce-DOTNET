namespace Reviews.DTOs
{
    public class CreateReviewDTO
    {
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; } = string.Empty;
        public List<MediaItemDTO>? Media { get; set; }
    }
}
