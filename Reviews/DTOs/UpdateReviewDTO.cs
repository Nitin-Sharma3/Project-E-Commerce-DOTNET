namespace Reviews.DTOs
{
    public class UpdateReviewDTO
    {
        public int Rating { get; set; }
        public string ReviewText { get; set; }
        public List<MediaItemDTO> Media { get; set; }
    }
}
