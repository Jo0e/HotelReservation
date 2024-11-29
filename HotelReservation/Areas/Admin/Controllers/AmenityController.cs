using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Models;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AmenityController : Controller
    {
        private readonly IRepository<Amenity> amenityRepository;

        public AmenityController(IRepository<Amenity> amenityRepository)
        {
            this.amenityRepository = amenityRepository;
        }

        // GET: AmenityController
        public ActionResult Index()
        {
            var Amenities = amenityRepository.Get();
            return View(Amenities);
        }

        // GET: AmenityController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AmenityController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Amenity amenity, IFormFile Img)
        {
            amenityRepository.CreateWithImage(amenity, Img, "amenities", nameof(amenity.Img));
            return RedirectToAction(nameof(Index));
        }

        // GET: AmenityController/Edit/5
        public ActionResult Edit(int id)
        {
            var amenity = amenityRepository.GetOne(where: e => e.Id == id);
            return View(amenity);
        }

        // POST: AmenityController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Amenity amenity, IFormFile Img)
        {
            var oldAmenity = amenityRepository.GetOne(where: a => a.Id == amenity.Id);
            amenityRepository.UpdateImage(amenity, Img, oldAmenity.Img, "amenities", nameof(amenity.Img));
            return RedirectToAction(nameof(Index));
        }

        // GET: AmenityController/Delete/5
        public ActionResult Delete(int id)
        {
            var amenity = amenityRepository.GetOne(where: a => a.Id == id);

            return View(amenity);
        }

        // POST: AmenityController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Amenity amenity)
        {
            amenityRepository.DeleteWithImage(amenity, "amenities",amenity.Img);
            amenityRepository.Commit();
            return RedirectToAction(nameof(Index));
        }
    }
}
