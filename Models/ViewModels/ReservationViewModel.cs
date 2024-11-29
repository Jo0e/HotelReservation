using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class ReservationViewModel
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public int RoomTypeId { get; set; }
        public int RoomCount { get; set; }
        public int NChildren { get; set; }
        public int NAdult { get; set; }
        public bool IncludesMeal { get; set; }
        public int? MealPrice { get; set; }
        public DateOnly CheckInDate { get; set; }
        public DateOnly CheckOutDate { get; set; }
        
       
        public string? CouponCode { get; set; }
        public decimal TotalPrice { get; set; } // New 
    }
}
