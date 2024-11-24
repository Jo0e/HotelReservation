namespace Models.Models
{
    // the data in this table wil be temporary it will be saved in Report details
    public class Reservation
    {
        public int Id { get; set; }
        public int RoomCount { get; set; }
        public int NChildren { get; set; }
        public int NAdult { get; set; }
        public DateOnly CheckInDate { get; set; }
        public DateOnly CheckOutDate { get; set; }
        public int? MealPrice { get; set; }
        public int? CouponId { get; set; }
        public Coupon Coupon { get; set; }
        public ICollection<ReservationRoom> ReservationRooms { get; set; }

    }

}
