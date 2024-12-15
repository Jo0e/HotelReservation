using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Utilities.Utility;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(SD.CompanyRole)]
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<AmenityController> logger;
        private readonly UserManager<IdentityUser> userManager;

        public AmenityController(IUnitOfWork unitOfWork, ILogger<AmenityController> logger , UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.userManager = userManager;
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
                Log(nameof(Create), nameof(amenity) + " " + $"{amenity.Name}");
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
                Log(nameof(Edit), nameof(amenity) + " " + $"{amenity.Name}");
                TempData["success"] = "Amenity updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(amenity);
        }

        public ActionResult Delete(int id)
        {
            var amenity = unitOfWork.AmenityRepository.GetOne(where: a => a.Id == id);
            if (amenity == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            unitOfWork.AmenityRepository.DeleteWithImage(amenity, "amenities", amenity.Img);
            Log(nameof(Delete), nameof(amenity) + " " + $"{amenity.Name}");
            unitOfWork.Complete();
            TempData["success"] = "Amenity deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        public async void Log(string action, string entity)
        {
            var user = await userManager.GetUserAsync(User);
            LoggerHelper.LogAdminAction(logger, user.Id, user.Email, action, entity);
        }
    }
}