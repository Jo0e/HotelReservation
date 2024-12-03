using Infrastructures.Repository;
using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Utilities.Profles;

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
                    return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                }

                var hotels = hotelRepository.Get(where: e => e.CompanyId == company.Id);
                return View(hotels.ToList());
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        public ActionResult Details(int id)
        {
            try
            {
                var user = userManager.GetUserId(User);
                var Hotels = hotelRepository.Get(
                 [e => e.HotelAmenities, e => e.Rooms, e => e.ImageLists],
                    where: e => e.CompanyId.ToString() == user && e.Id == id);
                return View(Hotels);
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        public ActionResult Create()
        {
            try
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
                    CompanyId = company.Id,
                   
                };

                ViewData["CompanyId"] = company?.Id;

                return View(hotel);
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        // POST: HotelsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Hotel hotel, IFormFile ImgFile)
        {
            try
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
                   
                    hotelRepository.CreateWithImage(hotel, ImgFile, "homeImage", "CoverImg");
                    return RedirectToAction(nameof(Index));
                }

                return View(hotel);
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        public ActionResult Edit(int id)
        {
            try
            {
                var user = userManager.GetUserName(User);
                var company = companyRepository.GetOne(where: e => e.UserName == user);
                var hotel = new Hotel
                {
                    CompanyId = company.Id,
                    ReportId = null
                };

                ViewData["CompanyId"] = company?.Id;

                var Hotel = hotelRepository.GetOne(where: e => e.Id == id);
                return View(Hotel);
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        // POST: HotelsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Hotel hotel, IFormFile ImgFile)
        {
            try
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
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        public ActionResult Delete(int id)
        {
            try
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
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        public ActionResult ImageList(int hotelId)
        {
            try
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
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        public ActionResult CreateImgList(int hotelId)
        {
            try
            {
                ViewBag.HotelId = hotelId;
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateImgList(ImageList imageList, ICollection<IFormFile> ImgUrl)
        {
            try
            {
                var hotel = hotelRepository.GetOne(where: e => e.Id == imageList.HotelId, tracked: false);
                imageListRepository.CreateImagesList(imageList, ImgUrl, hotel.Name);
                return RedirectToAction(nameof(ImageList));
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        public ActionResult DeleteImgList(int id)
        {
            try
            {
                var img = imageListRepository.GetOne(where: e => e.Id == id, tracked: false);
                var hotel = hotelRepository.GetOne(where: e => e.Id == img.HotelId, tracked: false);
                imageListRepository.DeleteImageList(id, hotel.Name);
                return RedirectToAction(nameof(ImageList));
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        public ActionResult DeleteAllImgList(int id)
        {
            try
            {
                var hotel = hotelRepository.GetOne(include: [e => e.ImageLists], where: e => e.Id == id, tracked: false);
                imageListRepository.DeleteHotelFolder(hotel.ImageLists, hotel.Name);
                return RedirectToAction(nameof(ImageList));
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }
    }
}
