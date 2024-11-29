using Infrastructures.Repository;
using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        private readonly IRoomRepository roomRepository;

        public BookingController(IReservationRepository reservationRepository, UserManager<IdentityUser> userManager, IRepository<ReservationRoom> reservationRoomRepository, IHotelRepository hotelRepository, ICouponRepository couponRepository
            , IRoomTypeRepository roomTypeRepository, IRoomRepository roomRepository)
        {
            this.reservationRepository = reservationRepository;
            this.userManager = userManager;
            this.reservationRoomRepository = reservationRoomRepository;
            this.hotelRepository = hotelRepository;
            this.couponRepository = couponRepository;
            this.roomTypeRepository = roomTypeRepository;
            this.roomRepository = roomRepository;
        }


        [HttpGet]
        public IActionResult Book(ReservationViewModel viewModel)
        {
            if (viewModel.ChildrenAge != null && viewModel.NChildren == viewModel.ChildrenAge.Count)
                foreach (var item in viewModel.ChildrenAge)
                    if (item >= 5)
                        viewModel.NAdult += 1;
             

            
            var rooms = roomRepository.Get(where: h => h.HotelId == viewModel.HotelId 
            && h.RoomType.MaxPersons>= viewModel.NAdult
            && h.RoomType.Type == viewModel.RoomType
            , include: [e => e.RoomType]);

            var roomsCount = rooms.Select(a => a.IsAvailable == true).Count();
            if (roomsCount <= viewModel.RoomCount )
                return View(rooms);

            return RedirectToAction("Index","Home");
        }


       
        public IActionResult Pay()
        {
            var appUser = userManager.GetUserId(User);
            var reservations = reservationRepository.Get(
             include: [r => r.ReservationRooms],
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
                        UnitAmountDecimal = 1000 * 100, //static             // reservation.TotalPrice * 100,
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
