namespace Models.Models
{
    public class Amenity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Img { get; set; }
        public ICollection<HotelAmenities> HotelAmenities { get; set; }

    }
}
