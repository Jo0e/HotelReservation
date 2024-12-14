using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using System.CodeDom;
using Utilities.Utility;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<IdentityUser> userManager;
        private readonly ILogger<AmenityController> logger;

        public AmenityController(IUnitOfWork unitOfWork,UserManager<IdentityUser> userManager,ILogger<AmenityController> logger)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.logger = logger;
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
            ModelState.Remove(nameof(Img));
            if (ModelState.IsValid)
            {
                unitOfWork.AmenityRepository.CreateWithImage(amenity, Img, "amenities", nameof(amenity.Img));
                Log(nameof(Create), nameof(amenity)+" "+$"{amenity.Name}");
                TempData["success"] = "Amenity created successfully!";
                return RedirectToAction(nameof(Index));
            } 
            return View(amenity);
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
            ModelState.Remove(nameof(Img));
            if (ModelState.IsValid)
            {
                var oldAmenity = unitOfWork.AmenityRepository.GetOne(where: a => a.Id == amenity.Id);
                if (oldAmenity == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                unitOfWork.AmenityRepository.UpdateImage(amenity, Img, oldAmenity.Img, "amenities", nameof(amenity.Img));
                TempData["success"] = "Amenity updated successfully!";
                Log(nameof(Edit), nameof(amenity) + " " + $"{amenity.Name}");
                return RedirectToAction(nameof(Index));
            }
            return View(amenity);
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
            Log(nameof(Delete), nameof(amenity) + " " + $"{amenity.Name}");
            unitOfWork.Complete();
            TempData["success"] = "Amenity deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async void Log(string action, string entity)
        {
            var user = await userManager.GetUserAsync(User);
            LoggerHelper.LogAdminAction(logger, user.Id,user.Email, action, entity);
        }
    }
}
