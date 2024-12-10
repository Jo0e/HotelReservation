using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class Amenity
    {
        public int Id { get; set; }
        [MinLength(3)]
        [MaxLength(100)]
        public string Name { get; set; }
        [ValidateNever]
        public string Img { get; set; }
        [ValidateNever] 
        public ICollection<HotelAmenities> HotelAmenities { get; set; }

    }
}
