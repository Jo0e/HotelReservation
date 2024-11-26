using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class CompanyViewModel
    {
        [ValidateNever]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [Required]
        public string Addres { get; set; }
        public string? ProfileImage { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Passwords { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Passwords))]
        public string ConfirmPassword { get; set; }

    }
}
