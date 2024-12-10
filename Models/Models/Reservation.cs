using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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
        public DateTime CheckInDate { get; set; }
        [CustomValidation(typeof(Reservation) , "ValidateCheckInOutDate")]
        public DateTime CheckOutDate { get; set; }
        public double TotalPrice { get; set; }
        public int? CouponId { get; set; }
        [ValidateNever]
        public Coupon Coupon { get; set; }
        public string UserId { get; set; }
        [ValidateNever]
        public ApplicationUser User { get; set; }
        public int HotelId { get; set; }
        [ValidateNever]
        public Hotel Hotel { get; set; }
        [ValidateNever]
        public ICollection<ReservationRoom> ReservationRooms { get; set; }




        public static ValidationResult ValidateCheckInOutDate(DateTime checkOutDate, ValidationContext context)
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
