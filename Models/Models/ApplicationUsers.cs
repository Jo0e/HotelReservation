using Microsoft.AspNetCore.Identity;

namespace Models.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string City { get; set; }
    }
}
