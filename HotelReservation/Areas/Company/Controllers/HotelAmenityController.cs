using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Models.Models;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    public class HotelAmenityController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public HotelAmenityController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // GET: AmenityController
        public ActionResult Index(int id)
        {
            try
            {
                if (id == 0)
                {
                    var hotelIdCookie = Request.Cookies["HotelIdCookie"];
                    if (hotelIdCookie == null || !int.TryParse(hotelIdCookie, out id))
                    {
                        return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                    }
                }
                else
                {
                    Response.Cookies.Append("HotelIdCookie", id.ToString());
                }

                var amenities = unitOfWork.HotelRepository.HotelsWithAmenities(id);
                if (amenities == null)
                {
                    return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                }

                return View(amenities);
            }
            catch (Exception )
            {

                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        // GET: AmenityController/Create
        public ActionResult Create(int hotelId)
        {
            try
            {
                var amenity = unitOfWork.AmenityRepository.Get();
                var hotelAmenities = unitOfWork.HotelAmenitiesRepository.Get(where: h => h.HotelId == hotelId).Select(a => a.AmenityId).ToList();
                ViewBag.Amenity = amenity;
                ViewBag.HotelAmenities = hotelAmenities;
                ViewBag.HotelId = hotelId;
                return View();
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
                    return RedirectToAction(nameof(Index));
                }
                if (amenitiesId.Count == 0)
                {
                    var toDelete = unitOfWork.HotelAmenitiesRepository.Get(where: h => h.HotelId == HotelId);
                    if (toDelete.Any())
                    {
                        unitOfWork.HotelAmenitiesRepository.DeleteRange(toDelete);
                        unitOfWork.Complete();
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
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }
    }
}