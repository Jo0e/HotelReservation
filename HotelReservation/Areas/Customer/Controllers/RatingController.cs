using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Models;

namespace HotelReservation.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class RatingController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<IdentityUser> userManager;

        public RatingController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> AddRating(int hotelId, double value)
        {
            var userId =  userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("NotFound");
            }
            var hasReservation = unitOfWork.ReservationRepository.Get(where: h => h.HotelId == hotelId && h.UserId == userId);
            if (!hasReservation.Any())
            {
                return Json(new { success = false, message = "You can only rate hotels you've stayed at." });
            }
            var existingRating = unitOfWork.RatingRepository.GetOne(where: r => r.HotelId == hotelId && r.UserId == userId);
            if (existingRating == null)
            {
                var rating = new Rating
                {
                    Value = value,
                    HotelId = hotelId,
                    UserId = userId,
                    Date = DateTime.Now
                };
                unitOfWork.RatingRepository.Create(rating);
                TempData["success"] = "Thank you for your rating!";
            }
            else
            {
                existingRating.Value = value;
                existingRating.Date = DateTime.Now;
                unitOfWork.RatingRepository.Update(existingRating);
            }
                unitOfWork.Complete();
            TempData["success"] = "Your rating has been updated!";

            return Json(new { success = true, message = "Thank you for your rating!" });

        }

        // Method to calculate average rating
        public double CalculateAverageRating(int hotelId)
        {
            var ratings = unitOfWork.RatingRepository.Get(where: r => r.HotelId == hotelId);
            if (ratings.Any())
            {
                return ratings.Average(r => r.Value);
            }
            return 0.0;
        }
    }

}
