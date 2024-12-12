using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class ReservationViewModel
    {
        public int HotelId { get; set; }
        public int NAdult { get; set; }
        public int? NChildren { get; set; }
        public List<int>? ChildrenAge { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public bool IncludesMeal { get; set; }
        public int RoomCount { get; set; }
        public Models.Type RoomType { get; set; }
        public string? CouponCode { get; set; }
    }
}
