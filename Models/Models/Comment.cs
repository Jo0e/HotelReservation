using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [ValidateNever]
        public ApplicationUser User { get; set; }
        [MaxLength(500)]
        public string CommentString { get; set; }
        public DateTime DateTime { get; set; }
        public int HotelId { get; set; }
        [ValidateNever]
        public Hotel Hotel { get; set; }

        public bool IsEdited { get; set; } = false;
        public int Likes { get; set; }  = 0;
        public ICollection<string> ReactionUsersId { get; set; }
        
    }
}
