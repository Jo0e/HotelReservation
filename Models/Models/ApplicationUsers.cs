using Microsoft.AspNetCore.Identity;

namespace Models.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string City { get; set; }
        public string? ProfileImage { get; set; }
    }
}
