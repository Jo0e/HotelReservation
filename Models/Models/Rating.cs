using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
    public class Rating
    {
        public int Id { get; set; }
        [Range(0, 5)]
        public double Value { get; set; }  
        public int HotelId { get; set; }
        [ValidateNever]
        public Hotel Hotel { get; set; }  
        public string UserId { get; set; }
        [ValidateNever]
        public ApplicationUser User { get; set; }
        public DateTime Date { get; set; }  
    }
}
