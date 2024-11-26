using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class Company
    {
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Addres { get; set; }
        public string? ProfileImage { get; set; }
        [Required]
        public string Passwords { get; set; }
        [ValidateNever]
        public ICollection<Hotel> Hotels { get; set; }
    }
}
