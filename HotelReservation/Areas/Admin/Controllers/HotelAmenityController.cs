using Infrastructures.Repository;
using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Stripe;
using Utilities.Utility;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HotelAmenityController : Controller
    {

        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<HotelAmenityController> logger;
        private readonly UserManager<IdentityUser> userManager;

        public HotelAmenityController(IUnitOfWork unitOfWork, ILogger<HotelAmenityController> logger, UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.userManager = userManager;
        }

        // GET: AmenityController
        public ActionResult Index(int id)
        {
            if (id != 0)
            {
                Response.Cookies.Append("HotelIdCookie", id.ToString());
            }
            else
            {
                id = int.Parse(Request.Cookies["HotelIdCookie"]);
            }
            var amenities = unitOfWork.HotelRepository.HotelsWithAmenities(id);
            return View(amenities);
        }


        // GET: AmenityController/Create
        public ActionResult Create(int hotelId)
        {
            var amenity = unitOfWork.AmenityRepository.Get();
            var hotelAmenities = unitOfWork.HotelAmenitiesRepository.Get(where: h => h.HotelId == hotelId).Select(a => a.AmenityId).ToList(); ;
            ViewBag.Amenity = amenity;
            ViewBag.HotelAmenities = hotelAmenities;
            ViewBag.HotelId = hotelId;
            return View();
        }

        // POST: AmenityController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int HotelId, List<int> amenitiesId)
        {

            if (HotelId != 0 && amenitiesId.Count != 0)
            {
                var toDelete = unitOfWork.HotelAmenitiesRepository.Get(where: h => h.HotelId == HotelId);
                unitOfWork.HotelAmenitiesRepository.DeleteRange(toDelete);
                foreach (var item in amenitiesId)
                {
                    var amenity = new HotelAmenities()
                    {
                        HotelId = HotelId,
                        AmenityId = item,
                    };
                    unitOfWork.HotelAmenitiesRepository.Create(amenity);

                }
                unitOfWork.Complete();
                Log(nameof(Create), "Assign Amenity to hotel");
                TempData["success"] = "Amenities successfully assigned to the hotel.";
                return RedirectToAction(nameof(Index));
            }
            if (amenitiesId.Count == 0)
            {
                var toDelete = unitOfWork.HotelAmenitiesRepository.Get(where: h => h.HotelId == HotelId);
                if (toDelete.Any())
                {
                    unitOfWork.HotelAmenitiesRepository.DeleteRange(toDelete);
                    unitOfWork.Complete();
                    Log(nameof(Create), "Clear Amenity from hotel");
                    TempData["success"] = "All amenities successfully removed from the hotel.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: AmenityController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int amenityId, int hotelId)
        {
            var hotelAmenities = new HotelAmenities { AmenityId = amenityId, HotelId = hotelId };
            unitOfWork.HotelAmenitiesRepository.Delete(hotelAmenities);
            unitOfWork.CompleteAsync();
            TempData["success"] = "Amenity successfully removed from the hotel.";
            Log(nameof(Delete), "Clear Amenity from hotel");
            return RedirectToAction(nameof(Index));
        }

        public async void Log(string action, string entity)
        {
            LoggerHelper.LogAdminAction(logger, User.Identity.Name, action, entity);

        }
    }
}
