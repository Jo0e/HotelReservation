using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Stripe.Checkout;

namespace HotelReservation.Areas.Customer.Controllers
{
    public class BookingController : Controller
    {
        private readonly IReservationRepository reservationRepository;
        private readonly UserManager<ApplicationUsers> userManager;

        public BookingController(IReservationRepository reservationRepository, UserManager<ApplicationUsers> userManager)
        {
            this.reservationRepository = reservationRepository;
            this.userManager = userManager;
        }

        public IActionResult Index(int id)
        {
            var appUser = userManager.GetUserId(User);

            var Booking = reservationRepository.Get([e => e.Hotel], e => e.ApplicationUserId == appUser && e.HotelId == id).ToList();

            return View(Booking);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Book(Reservation model)
        {

            var appUser = userManager.GetUserId(User);

            if (string.IsNullOrEmpty(appUser))
            {
                return RedirectToAction("Login", "Account");
            }


            model.ApplicationUserId = appUser;
            reservationRepository.Create(model);
            reservationRepository.Commit();
            TempData["Success"] = "Booking successfully created!";
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Delete(int hotelId)
        {
            var appUser = userManager.GetUserId(User);
            var cartDB = reservationRepository.GetOne(where: e => e.HotelId == hotelId && e.ApplicationUserId == appUser);

            if (cartDB != null)
            {
                reservationRepository.Delete(cartDB);
            }
            reservationRepository.Commit();

            return RedirectToAction("Index");
        }

        public IActionResult Pay()
        {
            var appUser = userManager.GetUserId(User);
            var cartDBs = reservationRepository.Get([e => e.Hotel], where: e => e.ApplicationUserId == appUser).ToList();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Cart/CheckOutSuccess",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Cart/CancelCheckout",
            };

            foreach (var item in cartDBs)
            {
                var result = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Hotel.Name,
                        },

                    },

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

            var shoppingCarts = reservationRepository.Get([e => e.Hotel, e => e.ApplicationUser], e => e.ApplicationUserId == appUser).ToList();


            foreach (var item in shoppingCarts)
            {
                reservationRepository.Delete(item);
            }
            reservationRepository.Commit();
            return View(shoppingCarts);

        }
        public IActionResult CancelCheckout()
        {
            TempData["error"] = "Your payment was cancelled. Please try again if you'd like to complete your order.";
            return View();
        }

    }
}
