using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Stripe;
using Utilities.Utility;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.CompanyRole)]
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
            try
            {
                Hotel amenities;
                if (id != 0)
                {
                    Response.Cookies.Append("HotelIdCookie", id.ToString());

                }
                else
                {
                    id = int.Parse(Request.Cookies["HotelIdCookie"]);

                }
                if (CheckAccesses(id))
                {
                    amenities = unitOfWork.HotelRepository.HotelsWithAmenities(id);
                    return View(amenities);
                }
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            catch
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        // GET: AmenityController/Create
        public ActionResult Create(int hotelId)
        {
            try
            {
                if (CheckAccesses(hotelId))
                {
                    var amenity = unitOfWork.AmenityRepository.Get();
                    var hotelAmenities = unitOfWork.HotelAmenitiesRepository.Get(where: h => h.HotelId == hotelId).Select(a => a.AmenityId).ToList();
                    ViewBag.Amenity = amenity;
                    ViewBag.HotelAmenities = hotelAmenities;
                    ViewBag.HotelId = hotelId;
                    return View();
                }
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            catch
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        // POST: AmenityController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int HotelId, List<int> amenitiesId)
        {
            try
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
                    TempData["success"] = "Amenities successfully assigned to the hotel.";
                    Log(nameof(Create), "Assign Amenity to hotel");
                    return RedirectToAction(nameof(Index));
                }
                if (amenitiesId.Count == 0)
                {
                    var toDelete = unitOfWork.HotelAmenitiesRepository.Get(where: h => h.HotelId == HotelId);
                    if (toDelete.Any())
                    {
                        unitOfWork.HotelAmenitiesRepository.DeleteRange(toDelete);
                        unitOfWork.Complete();
                        TempData["success"] = "All amenities successfully removed from the hotel.";
                        Log(nameof(Create), "Clear Amenity from hotel");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        // POST: AmenityController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int amenityId, int hotelId)
        {
            try
            {
                var hotelAmenities = new HotelAmenities { AmenityId = amenityId, HotelId = hotelId };
                unitOfWork.HotelAmenitiesRepository.Delete(hotelAmenities);
                unitOfWork.Complete();
                TempData["success"] = "Amenity successfully removed from the hotel.";
                Log(nameof(Delete), "Clear Amenity from hotel");
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }
        public async void Log(string action, string entity)
        {
            LoggerHelper.LogAdminAction(logger, User.Identity.Name, action, entity);

        }

        private bool CheckAccesses(int hotelId)
        {
            var user = userManager.GetUserName(User);
            var company = unitOfWork.CompanyRepository.GetOne(where: e => e.UserName == user, tracked: false);
            var hotel = unitOfWork.HotelRepository.GetOne(where: a => a.CompanyId == company.Id && a.Id == hotelId, tracked: false);
            return !(hotel == null);
        }
    }
}