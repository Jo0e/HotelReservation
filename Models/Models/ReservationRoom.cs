namespace Models.Models
{
    public class ReservationRoom
    {
        public int RoomId { get; set; }
        public Room Room { get; set; }
        public int ReservationID { get; set; }
        public Reservation Reservation { get; set; }
    }
}
