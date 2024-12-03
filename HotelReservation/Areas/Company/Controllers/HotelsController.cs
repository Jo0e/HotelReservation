using Infrastructures.Repository;
using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Utilities.Profles;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    public class HotelsController : Controller
    {
        private readonly IImageListRepository imageListRepository;
        private readonly IReportRepository reportRepository;
        private readonly IHotelRepository hotelRepository;
        private readonly ICompanyRepository companyRepository;
        private readonly UserManager<IdentityUser> userManager;
        public HotelsController(IImageListRepository imageListRepository, IReportRepository reportRepository, IHotelRepository hotelRepository, ICompanyRepository companyRepository, UserManager<IdentityUser> userManager)
        {
            this.imageListRepository = imageListRepository;
            this.reportRepository = reportRepository;
            this.hotelRepository = hotelRepository;
            this.companyRepository = companyRepository;
            this.userManager = userManager;
        }

        // GET: HotelsController
        public IActionResult Index()
        {
            try
            {
                var userName = userManager.GetUserName(User);

                if (string.IsNullOrEmpty(userName))
                {
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }

                var company = companyRepository.GetOne(where: e => e.UserName == userName);

                if (company == null)
                {
                    return NotFound("No associated company found for the current user.");
                }

                var hotels = hotelRepository.Get(where: e => e.CompanyId == company.Id);
                return View(hotels.ToList());
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home");
            }
        }


        public ActionResult Details(int id)
        {
            var user = userManager.GetUserId(User);
            var Hotels = hotelRepository.Get([e => e.HotelAmenities, e => e.Rooms, e => e.ImageLists], where: e => e.CompanyId.ToString() == user && e.Id == id);
            return View(Hotels);
        }

        public ActionResult Create()
        {
            var userName = userManager.GetUserName(User);
            var company = companyRepository.GetOne(where: e => e.UserName == userName);
            if (company == null)
            {
                ModelState.AddModelError("", "No company found for this user.");
                return View();
            }


            var hotel = new Hotel
            {
                CompanyId = company.Id
            };

            ViewData["CompanyId"] = company?.Id;

            return View(hotel);
        }



        // POST: HotelsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Hotel hotel, IFormFile ImgFile)
        {
            ModelState.Remove(nameof(ImgFile));
            if (ModelState.IsValid)
            {
                var userId = userManager.GetUserName(User);
                var company = companyRepository.GetOne(where: e => e.UserName == userId);

                if (company == null)
                {
                    ModelState.AddModelError("", "Invalid Company.");
                    return View(hotel);
                }

                hotel.CompanyId = company.Id;
                //ViewBag.Img = userId;
                hotelRepository.CreateWithImage(hotel, ImgFile, "homeImage", "CoverImg");
                return RedirectToAction(nameof(Index));
            }

            return View(hotel);
        }


        public ActionResult Edit(int id)
        {
            var user = userManager.GetUserName(User);

            var Comapny = companyRepository.GetOne(where: e => e.UserName == user);
            var hotel = new Hotel
            {
                CompanyId = Comapny.Id
            };
            ViewData["CompanyId"] = Comapny?.Id;

            var Hotel = hotelRepository.GetOne(where: e => e.Id == id);
            return View(Hotel);
        }

        // POST: HotelsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Hotel hotel, IFormFile ImgFile)
        {
            ModelState.Remove(nameof(ImgFile));
            if (ModelState.IsValid)
            {
                var oldHotel = hotelRepository.GetOne(where: e => e.Id == hotel.Id);
                hotelRepository.UpdateImage(hotel, ImgFile, oldHotel.CoverImg, "homeImage", "CoverImg");
                return RedirectToAction(nameof(Index));
            }
            return NotFound();

        }


        public ActionResult Delete(int id)
        {
            var oldHotel = hotelRepository.GetOne(where: e => e.Id == id);
            if (oldHotel != null)
            {
                imageListRepository.DeleteHotelFolder(oldHotel.ImageLists, oldHotel.Name);
            }
            hotelRepository.DeleteWithImage(oldHotel, "homeImage", oldHotel.CoverImg);

            hotelRepository.Commit();
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
            ViewBag.HotelName = hotelRepository.GetOne(where: n => n.Id == hotelId)?.Name;
            var imgs = imageListRepository.Get(where: p => p.HotelId == hotelId);
            return View(imgs);
        }
        public ActionResult CreateImgList(int hotelId)
        {
            ViewBag.HotelId = hotelId;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateImgList(ImageList imageList, ICollection<IFormFile> ImgUrl)
        {
            var hotel = hotelRepository.GetOne(where: e => e.Id == imageList.HotelId, tracked: false);
            imageListRepository.CreateImagesList(imageList, ImgUrl, hotel.Name);
            return RedirectToAction(nameof(ImageList));
        }
        public ActionResult DeleteImgList(int id)
        {
            var img = imageListRepository.GetOne(where: e => e.Id == id, tracked: false);
            var hotel = hotelRepository.GetOne(where: e => e.Id == img.HotelId, tracked: false);
            imageListRepository.DeleteImageList(id, hotel.Name);
            return RedirectToAction(nameof(ImageList));
        }

        public ActionResult DeleteAllImgList(int id)
        {
            var hotel = hotelRepository.GetOne(include: [e => e.ImageLists], where: e => e.Id == id, tracked: false);
            imageListRepository.DeleteHotelFolder(hotel.ImageLists, hotel.Name);
            return RedirectToAction(nameof(ImageList));
        }

    }
}