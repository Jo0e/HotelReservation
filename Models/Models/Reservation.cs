using System.ComponentModel.DataAnnotations;

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
        [CustomValidation(typeof(Reservation) , "ValidateCheckInOutDate")]
        public DateOnly CheckOutDate { get; set; }
        public int? CouponId { get; set; }
        public Coupon Coupon { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<ReservationRoom> ReservationRooms { get; set; }




        public static ValidationResult ValidateCheckInOutDate(DateOnly checkOutDate, ValidationContext context)
        {
            var instance = context.ObjectInstance as Reservation;
            if (instance == null || instance.CheckInDate <= checkOutDate)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("CheckOutDate must be after CheckInDate.");
        }
    }

}
