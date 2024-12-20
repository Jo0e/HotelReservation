using HotelReservation.Hubs;
using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.Models;
using Models.ViewModels;
using Newtonsoft.Json;
using Stripe.Checkout;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;

namespace HotelReservation.Areas.Customer.Controllers
{
    [Area("Customer")]

    public class BookingController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IHubContext<NotificationHub> hubContext;
        private readonly IUnitOfWork unitOfWork;


        public BookingController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager,
            IHubContext<NotificationHub> hubContext)
        {
            this.userManager = userManager;
            this.hubContext = hubContext;
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
        [Authorize]
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
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var tempNAdult = viewModel.NAdult;

            // Handel the NAdult 
            if (viewModel.ChildrenAge != null)
                foreach (var item in viewModel.ChildrenAge)
                    if (item > 5)
                        viewModel.NAdult++;

            var availableRoomsAfterCheckout = unitOfWork.RoomRepository.AvailableRooms(viewModel, typeModel);

            if (availableRoomsAfterCheckout == null || availableRoomsAfterCheckout.Count < viewModel.RoomCount)
            {
                var nearCheckoutRooms = unitOfWork.RoomRepository.NearCheckout(viewModel, typeModel);
                List<DateTime> dateTimes = new List<DateTime>();
                foreach (var near in nearCheckoutRooms)
                {
                    foreach (var room in near.ReservationRooms)
                    {
                        dateTimes.Add(room.Reservation.CheckOutDate);
                    }
                }

                if (nearCheckoutRooms != null && nearCheckoutRooms.Any())
                {

                    var firstCheckoutDate = dateTimes.FirstOrDefault();
                    var nextDay = firstCheckoutDate.AddDays(1); 

                   
                    TempData["NearCheckout"] = firstCheckoutDate.ToShortDateString();
                    TempData["NextDay"] = nextDay.ToShortDateString();
                }
                else
                {
                    TempData["ErrorMessage"] = "No rooms available for your selected dates or near checkout.";
                }

                // Redirect back to the Book GET method with hotelId
                return RedirectToAction(nameof(Book), viewModel);
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
                //NAdult = viewModel.NAdult,
                NAdult = tempNAdult,
                NChildren = viewModel.NChildren ?? 0,
                TotalPrice = totalPrice,
                UserId = appUserId,
                ReservationRooms = new List<ReservationRoom>(),
            };
            if (viewModel.CouponCode != null)
            {
                var coupon = unitOfWork.CouponRepository.GetOne(where: c => c.Code == viewModel.CouponCode);
                if (coupon != null && coupon.Limit > 0)
                {
                    totalPrice -= (int)(totalPrice * coupon.Discount / 100);
                    reservation.TotalPrice = totalPrice;
                    reservation.CouponId = coupon.Id;
                }
            }
            // Associate rooms with the reservation
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
            TempData["Success"] = "Booking successful!";
            return RedirectToAction(nameof(Pay), new { reservationId = reservation.Id });

        }
        
        [Authorize]
        public IActionResult Pay(int reservationId)
        {
            var appUser = userManager.GetUserId(User);
            if (appUser == null)
            {
                TempData["Error"] = "Unable to verify the user. Please log in.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Fetch the reservation from the database
            var reservation = unitOfWork.ReservationRepository.GetOne(
                include: [r => r.Hotel],
                where: r => r.UserId == appUser && r.Id == reservationId
            );

            if (reservation == null)
            {
                TempData["Error"] = "Reservation not found.";
                return RedirectToAction("Index", "Home");
            }

            // Ensure the reservation dates are valid
            var validationContext = new ValidationContext(reservation);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(reservation, validationContext, validationResults, true);

            if (!isValid)
            {
                TempData["Error"] = "Invalid reservation dates.";
                return RedirectToAction(nameof(Book), new { hotelId = reservation.HotelId });
            }

            // Recalculate total price (if necessary, especially if a coupon is applied)
            double totalPrice = reservation.TotalPrice;

            // Create the Stripe session options
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
                        Description = $"CheckInDate: {reservation.CheckInDate}  CheckOutDate: {reservation.CheckOutDate}"
                    },
                    UnitAmountDecimal = (decimal)(totalPrice * 100),  // Convert to smallest currency unit (e.g., cents)
                },
                Quantity = 1,
            }
        },
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Booking/CheckOutSuccess?reservationId={reservationId}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/Booking/CancelCheckout?reservationId={reservationId}",
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }


        public async Task<IActionResult> CheckOutSuccess(int reservationId)
        {
            var appUser = await userManager.GetUserAsync(User);

            if (appUser == null)
            {
                TempData["Error"] = "Unable to verify the user.";
                return RedirectToAction("Index", "Home");
            }
            var reservation = unitOfWork.ReservationRepository.GetOneReservation(reservationId);
            if (reservation != null && reservation.UserId == appUser.Id)
            {
                foreach (var room in reservation.ReservationRooms)
                {
                    room.Room.IsAvailable = false;
                    unitOfWork.RoomRepository.Update(room.Room);
                }
                reservation.Status = "Complete";
                unitOfWork.ReservationRepository.Update(reservation);
                if (reservation.CouponId != null)
                {
                    var coupon = unitOfWork.CouponRepository.GetOne(where: a => a.Id == reservation.CouponId, tracked: false);
                    if (coupon != null)
                    {
                        coupon.Limit--;
                        unitOfWork.CouponRepository.Update(coupon);
                    }
                }

                var reservationInfo = JsonConvert.SerializeObject(new
                {
                    reservationId,
                    reservation.RoomCount,
                    reservation.NAdult,
                    reservation.NChildren,
                    reservation.CheckInDate,
                    reservation.CheckOutDate,
                    reservation.TotalPrice,
                    User = new { appUser.Email },
                    Hotel = new { reservation.Hotel.Id, reservation.Hotel.Name },
                    reservation.Status
                });

                await hubContext.Clients.Group("Admins").SendAsync("NotifyAdminReservation", reservationInfo);

                SendReservationMessage(reservation,appUser.Id);
                
                await unitOfWork.CompleteAsync();
            }

            return View();
        }


        public IActionResult CancelCheckout(int reservationId)
        {

            var reservation = unitOfWork.ReservationRepository.GetOne(where: r => r.Id == reservationId);
            var reservationRoom = unitOfWork.ReservationRoomRepository.GetOne(where: e => e.ReservationID == reservationId);
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

        private void SendReservationMessage(Reservation reservation, string userId)
        {
            var message = new Message
            {
                MessageDateTime = DateTime.Now,
                Title = "Your Reservation",
                MessageString = $"Thank you for reservation with ID: {reservation.Id}\r\n" +
                        $"With Total Payment: {reservation.TotalPrice:C}\r\n" +
                        $"We Waiting for you at {reservation.Hotel.Name} on {reservation.CheckInDate:MMMM dd, yyyy}\r\n" +
                        $"Hotel Address: {reservation.Hotel.Address}",
                Description = "We hope you have safe travels, and enjoy your stay",
                UserId = userId,
            };
            unitOfWork.MessageRepository.Create(message);

        }

    }
}