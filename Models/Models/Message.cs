using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string MessageString { get; set; }
        public string Description { get; set; }
        public DateTime MessageDateTime { get; set; }
        public string UserId { get; set; }
        [ValidateNever]
        public ApplicationUser User { get; set; }
    }
}
