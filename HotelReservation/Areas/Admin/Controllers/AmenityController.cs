using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Models;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public AmenityController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // GET: AmenityController
        public ActionResult Index()
        {
            var Amenities = unitOfWork.AmenityRepository.Get();
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
                unitOfWork.AmenityRepository.CreateWithImage(amenity, Img, "amenities", nameof(amenity.Img));
                return RedirectToAction(nameof(Index));
        }

        // GET: AmenityController/Edit/5
        public ActionResult Edit(int id)
        {
            var amenity = unitOfWork.AmenityRepository.GetOne(where: e => e.Id == id);
            return View(amenity);
        }

        // POST: AmenityController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Amenity amenity, IFormFile Img)
        {
            var oldAmenity = unitOfWork.AmenityRepository.GetOne(where: a => a.Id == amenity.Id);
            unitOfWork.AmenityRepository.UpdateImage(amenity, Img, oldAmenity.Img, "amenities", nameof(amenity.Img));
            return RedirectToAction(nameof(Index));
        }

        // GET: AmenityController/Delete/5
        public ActionResult Delete(int id)
        {
            var amenity = unitOfWork.AmenityRepository.GetOne(where: a => a.Id == id);

            return View(amenity);
        }

        // POST: AmenityController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Amenity amenity)
        {
            unitOfWork.AmenityRepository.DeleteWithImage(amenity, "amenities",amenity.Img);
            unitOfWork.Complete();
            return RedirectToAction(nameof(Index));
        }
    }
}
