using Infrastructures.Repository;
using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Models;
using Models.ViewModels;

using Stripe.Checkout;

namespace HotelReservation.Areas.Customer.Controllers
{
    [Area("Customer")]

    public class BookingController : Controller
    {
        private readonly IReservationRepository reservationRepository;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IRepository<ReservationRoom> reservationRoomRepository;
        private readonly IHotelRepository hotelRepository;
        private readonly ICouponRepository couponRepository;
        private readonly IRoomTypeRepository roomTypeRepository;

        public BookingController(IReservationRepository reservationRepository, UserManager<IdentityUser> userManager,IRepository<ReservationRoom> reservationRoomRepository,IHotelRepository hotelRepository,ICouponRepository couponRepository,IRoomTypeRepository roomTypeRepository)
        {
            this.reservationRepository = reservationRepository;
            this.userManager = userManager;
            this.reservationRoomRepository = reservationRoomRepository;
            this.hotelRepository = hotelRepository;
            this.couponRepository = couponRepository;
            this.roomTypeRepository = roomTypeRepository;
        }

        
        [HttpGet]
        public IActionResult Book(int hotelId)
        {
            var hotel = hotelRepository.GetOne(
                include: [h => h.RoomTypes],
                where: h => h.Id == hotelId,
                tracked: false
            );

            if (hotel == null)
            {
                return RedirectToAction("NotFound");
            }

            ViewBag.RoomTypes = hotel.RoomTypes.Select(rt => new SelectListItem
            {
                Value = rt.Id.ToString(),
                Text = $"{rt.Type} - {rt.PricePN.ToString("C")}"
            }).ToList();

            return View(new ReservationViewModel { 
                HotelId = hotelId,
               
            });
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Book(ReservationViewModel model)
        {
            var appUser = userManager.GetUserId(User);

            if (string.IsNullOrEmpty(appUser))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var roomType = roomTypeRepository.GetOne(where: rt => rt.Id == model.RoomTypeId);
                if (roomType == null)
                {
                    TempData["Error"] = "Invalid room type selected.";
                    return RedirectToAction("Book", new { hotelId = model.HotelId });
                }

                double totalPrice = roomType.PricePN * model.RoomCount;
                if (model.IncludesMeal)
                {
                    totalPrice += model.RoomCount * 20; 
                }

                
                Coupon? appliedCoupon = null;
                if (!string.IsNullOrWhiteSpace(model.CouponCode))
                {
                    appliedCoupon = couponRepository.GetOne(where: c => c.Code == model.CouponCode);

                    if (appliedCoupon != null)
                    {
                        totalPrice -= totalPrice * (appliedCoupon.Discount / 100); 
                    }
                    else
                    {
                        TempData["Error"] = "Invalid or expired coupon code.";
                        return RedirectToAction("Book", new { hotelId = model.HotelId });
                    }
                }
                var reservation = new Reservation
                {
                    UserId = appUser,
                    NAdult = model.NAdult,
                    NChildren = model.NChildren,
                    RoomCount=model.RoomCount,
                    CheckInDate=model.CheckInDate,
                    CheckOutDate=model.CheckOutDate,
                    CouponId = appliedCoupon?.Id,

                };
                reservationRepository.Create(reservation);
                reservationRepository.Commit();

                for (int i = 0; i < model.RoomCount; i++)
                {
                    var reservationRoom = new ReservationRoom
                    {
                        ReservationID = reservation.Id,
                        RoomId = model.RoomTypeId
                    };
                    reservationRoomRepository.Create(reservationRoom);
                }

                reservationRoomRepository.Commit();
                TempData["Success"] = "Booking successfully created!";
                return RedirectToAction("Index", "Home");
            }

            var hotel = hotelRepository.GetOne(
                include: [h => h.RoomTypes],
                where: h => h.Id == model.HotelId,
                tracked: false
            );

            ViewBag.RoomTypes = hotel?.RoomTypes.Select(rt => new SelectListItem
            {
                Value = rt.Id.ToString(),
                Text = $"{rt.Type} - {rt.PricePN.ToString("C")}"
            }).ToList();

            TempData["Error"] = "Failed to create the booking. Please try again.";
            return View(model);
        }
        public IActionResult Pay()
        {
            var appUser = userManager.GetUserId(User);
            var reservations = reservationRepository.Get(
             include: [ r => r.ReservationRooms],
             where: r => r.UserId == appUser
             ).ToList();
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Booking/CheckOutSuccess",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/Booking/CancelCheckout",
            };

            foreach (var item in reservations)
            {
                var result = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Hotel:................., Room(s): {item.RoomCount}"
                        },
                        UnitAmountDecimal = 1000 *100, //static             // reservation.TotalPrice * 100,
                    },
                    Quantity = 1,
                };
                options.LineItems.Add(result);
            }

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }
        public IActionResult CheckOutSuccess()
        {
            var appUser = userManager.GetUserId(User);

            var reservations = reservationRepository.Get(
                        where: r => r.UserId == appUser
                        ).ToList();

            foreach (var reservation in reservations)
            {
                //reservation. = true; 
                //RoomTypeRepository.Update(reservation);
            }
            //RoomTypeRepository.Commit();

            TempData["Success"] = "Payment successful! Your reservations have been confirmed.";
            return View();

        }
        public IActionResult CancelCheckout()
        {
            TempData["Error"] = "Your payment was canceled. Please try again if you'd like to complete your booking.";
            return View();
        }


    }
}
