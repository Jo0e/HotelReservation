using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public double Value { get; set; }
        public int NVotes { get; set; }
    }
}
