using Microsoft.AspNetCore.Identity;

namespace Models.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string City { get; set; }
        public string? ProfileImage { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
