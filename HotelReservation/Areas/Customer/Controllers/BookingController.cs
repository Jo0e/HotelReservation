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


            //var roomsCount = rooms.Select(a => a.IsAvailable == true).Count();

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
            if (viewModel.CheckInDate < DateTime.Now.Date)
            {
                ModelState.AddModelError("CheckInDate", "Check-In date cannot be in the past.");

            }

            if (viewModel.CheckOutDate <= viewModel.CheckInDate)
            {
                ModelState.AddModelError("CheckOutDate", "Check-Out date must be after the Check-In date.");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid dates selected. Please correct the errors and try again.";
                return View(viewModel);
            }

            var appUserId = userManager.GetUserId(User);
            if (appUserId == null) return RedirectToAction("Login", "Account");

            var availableRooms = unitOfWork.RoomRepository.Get(
                where: r => r.HotelId == typeModel.HotelId
                         && r.IsAvailable
                         && r.RoomType != null
                         && r.RoomType.Type == typeModel.RoomType
                         && r.RoomType.PricePN == typeModel.PricePN
                          && (r.RoomType.MealPrice == typeModel.MealPrice || (r.RoomType.MealPrice == null && typeModel.MealPrice == null)),
                           include: [r => r.RoomType]).ToList();

            if (availableRooms == null || availableRooms.Count < viewModel.RoomCount)
            {
                TempData["ErrorMessage"] = "Not enough rooms are available. Please adjust your selection.";
                return RedirectToAction(nameof(Book), new { hotelId = viewModel.HotelId, roomType = viewModel.RoomType });
            }
            var selectedRoomType = availableRooms.First().RoomType;
            var totalPrice = selectedRoomType.PricePN * viewModel.RoomCount;

            if (viewModel.IncludesMeal && selectedRoomType.MealPrice.HasValue)
            {
                totalPrice += selectedRoomType.MealPrice.Value * viewModel.RoomCount;
            }

            if (!string.IsNullOrEmpty(viewModel.CouponCode))
            {
                var coupon = unitOfWork.CouponRepository.GetOne(where: c => c.Code == viewModel.CouponCode);
                if (coupon != null && coupon.Limit > 0)
                {
                    totalPrice -= (int)coupon.Discount;
                    coupon.Limit--;
                    unitOfWork.CouponRepository.Update(coupon);
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid or expired coupon.";
                    return View(viewModel);
                }
            }
            var reservation = new Reservation
            {
                HotelId = viewModel.HotelId,
                NAdult = viewModel.NAdult,
                NChildren = viewModel.NChildren ?? 0,
                CheckInDate = viewModel.CheckInDate,
                CheckOutDate = viewModel.CheckOutDate,
                RoomCount = viewModel.RoomCount,
                TotalPrice = totalPrice,
                UserId = appUserId
            };
            unitOfWork.ReservationRepository.Create(reservation);
            unitOfWork.Complete();

            for (int i = 0; i < viewModel.RoomCount; i++)
            {
                var room = availableRooms[i];
                var reservationRoom = new ReservationRoom
                {
                    RoomId = room.Id,
                    ReservationID = reservation.Id
                };
                // room.IsAvailable = false;
                unitOfWork.ReservationRoomRepository.Create(reservationRoom);
                unitOfWork.RoomRepository.Update(room);
            }

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
                        Name = $"Hotel: {reservation.Hotel.Name}, Rooms: {reservation.RoomCount}"
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


            //var reservations = unitOfWork.ReservationRepository.Get(
            //    include: [
            //r => r.ReservationRooms,
            //r => r.ReservationRooms.Select(rr => rr.Room)
            //    ],
            //    where: r => r.UserId == appUser
            //).ToList();

            var reservations = unitOfWork.ReservationRepository.GetReservationRoom(appUser);

            foreach (var reservation in reservations)
            {
                foreach (var reservationRoom in reservation.ReservationRooms)
                {
                    var room = reservationRoom.Room;
                    if (room != null)
                    {
                        room.IsAvailable = false;
                    }
                }
            }
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