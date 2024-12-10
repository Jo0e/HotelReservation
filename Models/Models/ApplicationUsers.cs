using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Models.ViewModels;

namespace Models.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string City { get; set; }
        public string? ProfileImage { get; set; }
        [ValidateNever]
        public IList<Message>? MessageString { get; set; } = new List<Message>();
        [ValidateNever]
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        [ValidateNever]
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
