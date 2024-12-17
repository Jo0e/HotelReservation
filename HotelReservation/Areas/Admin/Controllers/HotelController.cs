
using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Models;
using Stripe;
using Utilities.Utility;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class HotelController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<HotelController> logger;
        private readonly UserManager<IdentityUser> userManager;

        public HotelController(IUnitOfWork unitOfWork, ILogger<HotelController> logger,UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.userManager = userManager;
        }

        // GET: HotelController
        public ActionResult Index(string? search, int pageNumber = 1)
        {
            const int pageSize = 4;
            var hotels = unitOfWork.HotelRepository.Get(include: [p => p.company]);
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                hotels = hotels.Where(c =>
                    c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Address.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.City.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.company.UserName.Contains(search, StringComparison.OrdinalIgnoreCase));
            }
            var totalItems = hotels.Count();
            var pagedHotels = hotels
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.SearchText = search;

            return View(pagedHotels);
        }

        // GET: HotelController/Create
        public IActionResult Create()
        {
            ViewBag.CompanyId = unitOfWork.CompanyRepository.Get();
            return View();
        }

        // POST: HotelController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Hotel hotel, IFormFile ImgFile)
        {
            ModelState.Remove(nameof(ImgFile));
            if (ModelState.IsValid)
            {
                unitOfWork.HotelRepository.CreateWithImage(hotel, ImgFile, "homeImage", "CoverImg");
                TempData["success"] = "Hotel created successfully.";
                Log(nameof(Create), nameof(hotel) + " " + $"{hotel.Name}");
                return RedirectToAction(nameof(Index));
            }
            return View(hotel);

        }

        // GET: HotelController/Edit/5
        public ActionResult Edit(int id)
        {
            var hotel = unitOfWork.HotelRepository.GetOne(where: e => e.Id == id, include: [c => c.company]);
            ViewBag.CompanyId = unitOfWork.CompanyRepository.Get();
            return View(hotel);
        }

        // POST: HotelController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Hotel hotel, IFormFile ImgFile)
        {
            ModelState.Remove(nameof(ImgFile));
            ModelState.Remove(nameof(hotel.City));
            if (ModelState.IsValid)
            {
                var oldHotel = unitOfWork.HotelRepository.GetOne(where: e => e.Id == hotel.Id);
                if (oldHotel == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                hotel.City ??=oldHotel.City;
                unitOfWork.HotelRepository.UpdateImage(hotel, ImgFile, oldHotel.CoverImg, "homeImage", "CoverImg");
                TempData["success"] = "Hotel updated successfully.";
                Log(nameof(Edit), nameof(hotel) + " " + $"{hotel.Name}");
                return RedirectToAction(nameof(Index));
            }
            return NotFound();

        }

        // GET: HotelController/Delete/5
        public ActionResult Delete(int id)
        {
            var hotel = unitOfWork.HotelRepository.GetOne(where: e => e.Id == id, include: [c => c.company]);
            if (hotel == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            return View(hotel);
        }

        // POST: HotelController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Hotel hotel)
        {
            var oldHotel = unitOfWork.HotelRepository.GetOne(where: e => e.Id == hotel.Id);
            if (oldHotel == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            unitOfWork.HotelRepository.DeleteWithImage(oldHotel, "homeImage", oldHotel.CoverImg);
            unitOfWork.Complete();
            TempData["success"] = "Hotel deleted successfully.";
            Log(nameof(Delete), nameof(hotel));
            return RedirectToAction(nameof(Index));
        }

        public ActionResult ImageList(int hotelId)
        {
            if (hotelId != 0)
            {
                Response.Cookies.Append("HotelId", hotelId.ToString());
            }
            if (hotelId == 0)
            {
                hotelId = int.Parse(Request.Cookies["HotelId"]);
            }
            ViewBag.HotelId = hotelId;
            ViewBag.HotelName = unitOfWork.HotelRepository.GetOne(where: n => n.Id == hotelId).Name;
            var imgs = unitOfWork.ImageListRepository.Get(where: p => p.HotelId == hotelId);
            return View(imgs);
        }
        public ActionResult CreateImgList(int hotelId)
        {
            //var hotelId = int.Parse(Request.Cookies["HotelId"]);
            ViewBag.HotelId = hotelId;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateImgList(ImageList imageList, ICollection<IFormFile> ImgUrl)
        {
            var hotel = unitOfWork.HotelRepository.GetOne(where: e => e.Id == imageList.HotelId, tracked: false);
            if (hotel == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            unitOfWork.ImageListRepository.CreateImagesList(imageList, ImgUrl, hotel.Name);
            Log(nameof(CreateImgList), $"imageList {hotel.Name}");
            TempData["success"] = "Images added successfully.";
            return RedirectToAction(nameof(ImageList));
        }

        public ActionResult DeleteImgList(int id)
        {
            var img = unitOfWork.ImageListRepository.GetOne(where: e => e.Id == id , tracked:false);
            if (img == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            var hotel = unitOfWork.HotelRepository.GetOne(where: e => e.Id == img.HotelId , tracked: false);
            if (hotel == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            unitOfWork.ImageListRepository.DeleteImageList(id,hotel.Name);
            Log(nameof(DeleteImgList), $"imageList {hotel.Name}");
            TempData["success"] = "Image deleted successfully.";
            return RedirectToAction(nameof(ImageList));
        }

        public ActionResult DeleteAllImgList(int hotelId) 
        {
            var hotel = unitOfWork.HotelRepository.GetOne(include: [e=>e.ImageLists],where: e => e.Id == hotelId, tracked: false);
            if (hotel == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            TempData["success"] = "All images deleted successfully.";
            unitOfWork.ImageListRepository.DeleteHotelFolder(hotel.ImageLists,hotel.Name);
            Log(nameof(DeleteAllImgList), $"imageList {hotel.Name}");
            return RedirectToAction(nameof(ImageList));
        }

        public async void Log(string action, string entity)
        {
            LoggerHelper.LogAdminAction(logger, User.Identity.Name, action, entity);

        }
    }
}
