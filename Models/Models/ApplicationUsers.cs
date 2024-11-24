using Microsoft.AspNetCore.Identity;

namespace Models.Models
{
    public class ApplicationUsers : IdentityUser
    {
        public string City { get; set; }
    }
}
