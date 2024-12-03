﻿using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models.Models;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    public class HotelAmenityController : Controller
    {
        private readonly IHotelRepository hotelRepository;
        private readonly IHotelAmenitiesRepository hotelAmenitiesRepository;
        private readonly IRepository<Amenity> amenityRepository;

        public HotelAmenityController(IHotelRepository hotelRepository, IHotelAmenitiesRepository hotelAmenitiesRepository
            , IRepository<Amenity> amenityRepository)
        {
            this.hotelRepository = hotelRepository;
            this.hotelAmenitiesRepository = hotelAmenitiesRepository;
            this.amenityRepository = amenityRepository;
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
                    amenities = hotelRepository.HotelsWithAmenities(id);
                    return View(amenities);
                }
                else if (id == 0)
                {
                    var hotelId = int.Parse(Request.Cookies["HotelIdCookie"]);
                    amenities = hotelRepository.HotelsWithAmenities(hotelId);
                    return View(amenities);
                }
            }
            catch
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }

            return NotFound();
        }

        // GET: AmenityController/Create
        public ActionResult Create(int hotelId)
        {
            try
            {
                var amenity = amenityRepository.Get();
                var hotelAmenities = hotelAmenitiesRepository.Get(where: h => h.HotelId == hotelId).Select(a => a.AmenityId).ToList();
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
                    var toDelete = hotelAmenitiesRepository.Get(where: h => h.HotelId == HotelId);
                    hotelAmenitiesRepository.DeleteRange(toDelete);
                    foreach (var item in amenitiesId)
                    {
                        var amenity = new HotelAmenities()
                        {
                            HotelId = HotelId,
                            AmenityId = item,
                        };
                        hotelAmenitiesRepository.Create(amenity);
                    }
                    hotelAmenitiesRepository.Commit();
                    return RedirectToAction(nameof(Index));
                }
                if (amenitiesId.Count == 0)
                {
                    var toDelete = hotelAmenitiesRepository.Get(where: h => h.HotelId == HotelId);
                    if (toDelete.Any())
                    {
                        hotelAmenitiesRepository.DeleteRange(toDelete);
                        hotelAmenitiesRepository.Commit();
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
                hotelAmenitiesRepository.Delete(hotelAmenities);
                hotelAmenitiesRepository.Commit();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }
    }
}