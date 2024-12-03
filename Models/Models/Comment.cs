using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string CommentString { get; set; }
        public DateTime DateTime { get; set; }
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }

        public bool IsEdited { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }

        public ICollection<Reply> Replies { get; set; } = new List<Reply>();
    }
}
