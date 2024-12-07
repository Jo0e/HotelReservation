using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
    public class Reply
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string ReplyString { get; set; }
        public DateTime DateTime { get; set; }
        public int CommentId { get; set; }
        public Comment Comment { get; set; }
        public bool IsEdited { get; set; } = false;
        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
    }
}
