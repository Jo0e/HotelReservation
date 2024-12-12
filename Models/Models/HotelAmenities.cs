using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Models.Models
{

    public class HotelAmenities
    {
        public int HotelId { get; set; }
        public int AmenityId { get; set; }
        [ValidateNever]
        public Amenity Amenity { get; set; }
        [ValidateNever]
        public Hotel Hotel { get; set; }

    }
}
