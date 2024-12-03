using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Models.Models
{
    public class ReservationRoom
    {
        public int RoomId { get; set; }
        [ValidateNever]
        public Room Room { get; set; }
        public int ReservationID { get; set; }
        [ValidateNever]
        public Reservation Reservation { get; set; }
    }
}
