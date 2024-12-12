using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class TypeViewModel
    {
        public int HotelId { get; set; }
        public Models.Type RoomType { get; set; }
        public int PricePN { get; set; }
        public int? MealPrice { get; set; }
        public int? MaxPersons { get; set; }
    }
}
