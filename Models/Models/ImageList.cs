namespace Models.Models
{
    public class ImageList
    {
        public int Id { get; set; }
        public string ImgUrl { get; set; } = string.Empty;
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }

    }
}
