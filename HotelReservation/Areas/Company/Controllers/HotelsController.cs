using Infrastructures.Repository;
using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Models.Models;
using Utilities.Profles;
using Utilities.Utility;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.CompanyRole)]
    public class HotelsController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<IdentityUser> userManager;
        private readonly ILogger<HotelsController> logger;
        private readonly IHubContext<HotelHub> hubContext;

        public HotelsController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, ILogger<HotelsController> logger, IHubContext<HotelHub> hubContext)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.logger = logger;
            this.hubContext = hubContext;
        }

        // GET: HotelsController
        public IActionResult Index(string? search, int pageNumber = 1)
        {
            try
            {
                var userName = userManager.GetUserName(User);

                if (string.IsNullOrEmpty(userName))
                {
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }

                var company = unitOfWork.CompanyRepository.GetOne(where: e => e.UserName == userName);

                if (company == null)
                {
                    return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                }

                var hotels = unitOfWork.HotelRepository.Get(where: e => e.CompanyId == company.Id);
                const int pageSize = 8;
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
                return View(pagedHotels.ToList());
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
                var Hotels = unitOfWork.HotelRepository.Get(
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
                var company = unitOfWork.CompanyRepository.GetOne(where: e => e.UserName == userName);
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
        public async Task<ActionResult> Create(Hotel hotel, IFormFile ImgFile)
        {
            try
            {
                ModelState.Remove(nameof(ImgFile));
                if (ModelState.IsValid)
                {
                    var userId = userManager.GetUserName(User);
                    var company = unitOfWork.CompanyRepository.GetOne(where: e => e.UserName == userId);

                    if (company == null)
                    {
                        ModelState.AddModelError("", "Invalid Company.");
                        return View(hotel);
                    }

                    hotel.CompanyId = company.Id;

                    unitOfWork.HotelRepository.CreateWithImage(hotel, ImgFile, "homeImage", "CoverImg");
                   
                    await hubContext.Clients.All.SendAsync("HotelCreated", hotel);

                    // Notify clients

                    //Log(nameof(Create), nameof(hotel) + " " + $"{hotel.Name}");
                    TempData["success"] = "Hotel created successfully.";
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
                var company = unitOfWork.CompanyRepository.GetOne(where: e => e.UserName == user);
                var hotel = new Hotel
                {
                    CompanyId = company.Id,
                    ReportId = null
                };

                ViewData["CompanyId"] = company?.Id;

                var Hotel = unitOfWork.HotelRepository.GetOne(where: e => e.Id == id && e.CompanyId == company.Id);
                if (Hotel != null)
                {
                    return View(Hotel);
                }
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });

            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        // POST: HotelsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Hotel hotel, IFormFile ImgFile)
        {
            try
            {
                ModelState.Remove(nameof(ImgFile));
                if (ModelState.IsValid)
                {
                    var oldHotel = unitOfWork.HotelRepository.GetOne(where: e => e.Id == hotel.Id);
                    if (oldHotel == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                    unitOfWork.HotelRepository.UpdateImage(hotel, ImgFile, oldHotel.CoverImg, "homeImage", "CoverImg");
                    await hubContext.Clients.All.SendAsync("HotelUpdated", hotel); // Notify clients

                    //Log(nameof(Edit), nameof(hotel) + " " + $"{hotel.Name}");
                    TempData["success"] = "Hotel updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                return View(hotel);
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }
        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var oldHotel = unitOfWork.HotelRepository.GetOne(where: e => e.Id == id);
                if (oldHotel != null)
                {
                    unitOfWork.ImageListRepository.DeleteHotelFolder(oldHotel.ImageLists, oldHotel.Name);
                }
                unitOfWork.HotelRepository.DeleteWithImage(oldHotel, "homeImage", oldHotel.CoverImg);
                // Log(nameof(Delete), "hotel" + " " + $"{oldHotel.Name}");
                unitOfWork.Complete();
                await hubContext.Clients.All.SendAsync("HotelDeleted", id); // Notify clients

                TempData["success"] = "Hotel deleted successfully.";
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
                var user = userManager.GetUserName(User);
                var company = unitOfWork.CompanyRepository.GetOne(where: e => e.UserName == user);
                ViewBag.HotelName = unitOfWork.HotelRepository.GetOne(where: n => n.Id == hotelId && n.CompanyId == company.Id)?.Name;
                if (ViewBag.HotelName != null)
                {
                    var imgs = unitOfWork.ImageListRepository.Get(where: p => p.HotelId == hotelId);
                    return View(imgs);
                }
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
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
                var user = userManager.GetUserName(User);
                var company = unitOfWork.CompanyRepository.GetOne(where: e => e.UserName == user, tracked: false);
                var hotel = unitOfWork.HotelRepository.GetOne(where: e => e.Id == imageList.HotelId && e.CompanyId == company.Id, tracked: false);
                if (hotel == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                //Log(nameof(CreateImgList), nameof(imageList) + " " + $"{hotel.Name}");
                unitOfWork.ImageListRepository.CreateImagesList(imageList, ImgUrl, hotel.Name);
                TempData["success"] = "Images added successfully.";

                return RedirectToAction(nameof(ImageList));
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }
        [HttpPost]
        public ActionResult DeleteImgList(int id)
        {
            try
            {
                var img = unitOfWork.ImageListRepository.GetOne(where: e => e.Id == id, tracked: false);
                if (img == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                var hotel = unitOfWork.HotelRepository.GetOne(where: e => e.Id == img.HotelId, tracked: false);
                if (hotel == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                //Log(nameof(DeleteImgList), "imageList" + " " + $"{hotel.Name}");
                unitOfWork.ImageListRepository.DeleteImageList(id, hotel.Name);
                return RedirectToAction(nameof(ImageList));
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }
        [HttpPost]
        public ActionResult DeleteAllImgList(int id)
        {
            try
            {
                var hotel = unitOfWork.HotelRepository.GetOne(include: [e => e.ImageLists], where: e => e.Id == id, tracked: false);
                if (hotel == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                //Log(nameof(DeleteAllImgList), "imageList" + " " + $"{hotel.Name}");
                unitOfWork.ImageListRepository.DeleteHotelFolder(hotel.ImageLists, hotel.Name);
                TempData["success"] = "All images deleted successfully.";
                return RedirectToAction(nameof(ImageList));
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }
        //public async void Log(string action, string entity)
        //{
        //    var user = await userManager.GetUserAsync(User);
        //    LoggerHelper.LogAdminAction(logger, user.Id, user.Email, action, entity);
        //}
    }
}
