using Infrastructures.Repository;
using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.Models;
using Models.ViewModels;

using Stripe.Checkout;
using System.Linq.Expressions;

namespace HotelReservation.Areas.Customer.Controllers
{
    [Area("Customer")]

    public class BookingController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUnitOfWork unitOfWork;


        public BookingController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;

        }

        [HttpGet]
        public IActionResult Filter(int hotelId, Models.Models.Type RoomType)
        {
            var rooms = unitOfWork.RoomRepository.Get(where: h => h.HotelId == hotelId
           && h.RoomType.Type == RoomType
           , include: [e => e.RoomType]);


            var roomsCount = rooms.Select(a => a.IsAvailable == true).Count();
            return View(rooms);
        }

        [HttpGet]
        public IActionResult Book(TypeViewModel typeModel)
        {

            var hotel = unitOfWork.HotelRepository.GetOne(where: e => e.Id == typeModel.HotelId);
            if (hotel == null) return NotFound();

            int availableRoomsCount = unitOfWork.RoomRepository
        .Get(where: r => r.HotelId == typeModel.HotelId
                         && r.IsAvailable
                         && r.RoomType != null
                         && r.RoomType.Type == typeModel.RoomType
                         && r.RoomType.PricePN == typeModel.PricePN
                          && (r.RoomType.MealPrice == typeModel.MealPrice || (r.RoomType.MealPrice == null && typeModel.MealPrice == null)))
        .Count();


            ViewBag.Type = typeModel;
            ViewBag.availableRooms = availableRoomsCount;

            return View(hotel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Book(ReservationViewModel viewModel, TypeViewModel typeModel)
        {
            var appUserId = userManager.GetUserId(User);
            if (appUserId == null)
                return RedirectToAction("Login", "Account");

            // Fetch available rooms
            // Fetch rooms that will be available after the checkout date
            var availableRoomsAfterCheckout = unitOfWork.RoomRepository.Get(
                where: r => r.HotelId == typeModel.HotelId
                         && r.RoomType.Type == typeModel.RoomType
                         && r.RoomType.PricePN == typeModel.PricePN
                         && (!viewModel.IncludesMeal || r.RoomType.MealPrice == typeModel.MealPrice)
                         && (r.ReservationRooms == null || !r.ReservationRooms.Any() ||
                             r.ReservationRooms.All(rr =>
                                 rr.Reservation.CheckOutDate < viewModel.CheckInDate ||
                                 rr.Reservation.CheckInDate > viewModel.CheckOutDate
                             )
                         ),
                include: [r => r.RoomType]
            ).ToList();

            // Fetch the next available rooms after CheckOutDate
            var nextAvailableRooms = unitOfWork.RoomRepository.Get(
                    where: r => r.HotelId == typeModel.HotelId
                             && r.RoomType.Type == typeModel.RoomType
                             && r.RoomType.PricePN == typeModel.PricePN
                             && !r.IsAvailable,
                    include: [r => r.ReservationRooms]).ToList();

               
              
                // Check if there are enough available rooms after checkout
                if (availableRoomsAfterCheckout == null || availableRoomsAfterCheckout.Count < viewModel.RoomCount)
                {
                    TempData["ErrorMessage"] = "No rooms available for your selected dates or near future.";
                    return RedirectToAction(nameof(Book), new { hotelId = viewModel.HotelId });
                }

                // Calculate total price
                var totalMealPrice = viewModel.IncludesMeal ? (typeModel.MealPrice ?? 0) : 0;
                var totalPrice = (typeModel.PricePN + totalMealPrice) * viewModel.RoomCount *
                                 (viewModel.CheckOutDate - viewModel.CheckInDate).Days;

                // Create a reservation
                var reservation = new Reservation
                {
                    HotelId = viewModel.HotelId,
                    CheckInDate = viewModel.CheckInDate,
                    CheckOutDate = viewModel.CheckOutDate,
                    RoomCount = viewModel.RoomCount,
                    NAdult = viewModel.NAdult,
                    NChildren = viewModel.NChildren ?? 0,
                    TotalPrice = totalPrice,
                    UserId = appUserId,
                    ReservationRooms = new List<ReservationRoom>()
                };

                // Associate rooms with the reservation (now from the available ones after checkout)
                var allocatedRooms = availableRoomsAfterCheckout.Take(viewModel.RoomCount).ToList();
                foreach (var room in allocatedRooms)
                {
                    reservation.ReservationRooms.Add(new ReservationRoom
                    {
                        RoomId = room.Id,
                        Reservation = reservation
                    });
                }

                // Save the reservation
                unitOfWork.ReservationRepository.Create(reservation);
                unitOfWork.Complete();

                TempData["SuccessMessage"] = "Booking successful!";
                return RedirectToAction(nameof(Pay), new { reservationId = reservation.Id });

            
        }



        public IActionResult Pay(int reservationId)
        {
            var appUser = userManager.GetUserId(User);
            if (appUser == null)
            {
                TempData["Error"] = "Unable to verify the user. Please log in.";
                return RedirectToAction("Login", "Account");
            }

            var reservation = unitOfWork.ReservationRepository.GetOne(
                include: [r => r.Hotel],
                where: r => r.UserId == appUser && r.Id == reservationId
            );

            if (reservation == null)
            {
                TempData["Error"] = "Reservation not found.";
                return RedirectToAction("Index", "Home");
            }

            // Check date validity
            if (reservation.CheckInDate < DateTime.Now.Date || reservation.CheckInDate >= reservation.CheckOutDate)
            {
                TempData["Error"] = "Cannot proceed with payment. Invalid reservation dates.";
                return RedirectToAction("Index", "Home");
            }
            // Create Stripe payment session
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "egp",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = $"Hotel: {reservation.Hotel.Name}, Rooms: {reservation.RoomCount}",
                        Description=$"CheckInDate: {reservation.CheckInDate}  CheckOutDate: {reservation.CheckOutDate}"
                    },
                    UnitAmountDecimal = (decimal?)(reservation.TotalPrice * 100),
                },
                Quantity = 1,
            }
        },
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Booking/CheckOutSuccess",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/Booking/CancelCheckout",
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }
       
        public IActionResult CheckOutSuccess()
        {
            var appUser = userManager.GetUserId(User);

            if (appUser == null)
            {
                TempData["Error"] = "Unable to verify the user.";
                return RedirectToAction("Index", "Home");
            }

            var reservations = unitOfWork.ReservationRoomRepository.Get(
                include: new Expression<Func<ReservationRoom, object>>[] { e => e.Reservation, e => e.Room },
                where: e => e.Reservation.UserId == appUser
            );

            foreach (var reservation in reservations)
            {
                
                if (reservation != null && reservation.Reservation.CheckInDate.Date == DateTime.Now.Date)
                {
                    reservation.Room.IsAvailable = false;
                    unitOfWork.RoomRepository.Update(reservation.Room);
                }
            }

            unitOfWork.Complete();

            TempData["Success"] = "Payment successful! Your reservations have been confirmed.";
            return View();
        }


        public IActionResult CancelCheckout(int reservationId)
        {
            
            var reservation = unitOfWork.ReservationRepository.GetOne(where:r => r.Id == reservationId);
            var reservationRoom = unitOfWork.ReservationRoomRepository.GetOne(where:e=>e.ReservationID== reservationId);
            if (reservationRoom != null)
            {
                unitOfWork.ReservationRoomRepository.Delete(reservationRoom);
                unitOfWork.Complete();
            }

            if (reservation != null)
            {
                unitOfWork.ReservationRepository.Delete(reservation);
                unitOfWork.Complete();
            }

            TempData["Error"] = "Your payment was canceled. Please try again if you'd like to complete your booking.";
            return View();
        }



    }
}