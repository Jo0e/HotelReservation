using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;

namespace HotelReservation.Areas.Customer.Controllers
{
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRating(int hotelId, double value)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("NotFound");
            }

            var rating = new Rating
            {
                Value = value,
                HotelId = hotelId,
                UserId = user.Id,
                Date = DateTime.Now
            };

            unitOfWork.RatingRepository.Create(rating);
            unitOfWork.Complete();

            return RedirectToAction("Details", "Hotel", new { id = hotelId });
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
