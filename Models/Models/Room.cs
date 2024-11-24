using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Models.Models
{
    public class Room
    {
        public int Id { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int HotelId { get; set; }
        [ValidateNever]
        public Hotel Hotel { get; set; }
        public int RoomTypeId { get; set; }
        [ValidateNever]
        public RoomType RoomType { get; set; }
        [ValidateNever]
        public ICollection<ReservationRoom> ReservationRooms { get; set; } = new List<ReservationRoom>();
    }
}
