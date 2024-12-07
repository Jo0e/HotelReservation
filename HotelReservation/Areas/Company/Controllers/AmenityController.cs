using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using System;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
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
            try
            {
                var Amenities = unitOfWork.AmenityRepository.Get();
                return View(Amenities);
            }
            catch (Exception ex)
            {
                // Log exception (optional)
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        // GET: AmenityController/Create
        public ActionResult Create()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                // Log exception (optional)
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        // POST: AmenityController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Amenity amenity, IFormFile Img)
        {
            try
            {
                unitOfWork.AmenityRepository.CreateWithImage(amenity, Img, "amenities", nameof(amenity.Img));
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log exception (optional)
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        // GET: AmenityController/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var amenity = unitOfWork.AmenityRepository.GetOne(where: e => e.Id == id);
                return View(amenity);
            }
            catch (Exception ex)
            {
                // Log exception (optional)
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        // POST: AmenityController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Amenity amenity, IFormFile Img)
        {
            try
            {
                var oldAmenity = unitOfWork.AmenityRepository.GetOne(where: a => a.Id == amenity.Id);
                unitOfWork.AmenityRepository.UpdateImage(amenity, Img, oldAmenity.Img, "amenities", nameof(amenity.Img));
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log exception (optional)
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        // POST: AmenityController/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                var amenity = unitOfWork.AmenityRepository.GetOne(where: a => a.Id == id);
                unitOfWork.AmenityRepository.DeleteWithImage(amenity, "amenities", amenity?.Img);
                unitOfWork.Complete();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log exception (optional)
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }
    }
}
