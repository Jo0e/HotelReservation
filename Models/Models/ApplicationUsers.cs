using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Models.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string City { get; set; }
        public string? ProfileImage { get; set; }
        [ValidateNever]
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        [ValidateNever]
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
