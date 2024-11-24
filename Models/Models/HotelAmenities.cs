using System.ComponentModel.DataAnnotations;

namespace Models.Models
{

    public class HotelAmenities
    {
        public int HotelId { get; set; }
        public int AmenityId { get; set; }
        public Amenity Amenity { get; set; }
        public Hotel Hotel { get; set; }

    }
}
