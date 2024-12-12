using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class Hotel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter name")]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        [Range(1,5)]
        public int Stars { get; set; }
        [ValidateNever]
        public string CoverImg { get; set; }

        public int CompanyId { get; set; }
        [ValidateNever]
        public Company company { get; set; }

        public int? ReportId { get; set; }
        [ValidateNever]
        public Report Report { get; set; }

        [ValidateNever]
        public ICollection<Comment> Comments { get; set; }
        [ValidateNever]
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>(); 
        [ValidateNever]
        public List<ImageList> ImageLists { get; set; } = new List<ImageList>();

        [ValidateNever]
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        [ValidateNever]
        public ICollection<RoomType>  RoomTypes { get; set; } = new List<RoomType>();

        [ValidateNever]
        public ICollection<HotelAmenities> HotelAmenities { get; set; } = new List<HotelAmenities>();
        [ValidateNever]
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        
    }
}
