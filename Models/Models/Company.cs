using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class Company
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter name")]
        public string UserName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Addres { get; set; }
        [ValidateNever]
        public string? ProfileImage { get; set; }
        [Required]
        public string Passwords { get; set; }
        [ValidateNever]
        public ICollection<Hotel> Hotels { get; set; }
    }
}
