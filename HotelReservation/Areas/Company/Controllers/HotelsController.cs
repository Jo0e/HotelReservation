
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
        private readonly IReportRepository reportRepository;
        private readonly IHotelRepository hotelRepository;
        private readonly ICompanyRepository companyRepository;
        private readonly UserManager<IdentityUser> userManager;
        public HotelsController(IReportRepository reportRepository,IHotelRepository hotelRepository,ICompanyRepository companyRepository,UserManager<IdentityUser> userManager)
        {
            this.reportRepository = reportRepository;
            this.hotelRepository = hotelRepository;
            this.companyRepository = companyRepository;
            this.userManager = userManager;
        }
        
        // GET: HotelsController
        public ActionResult Index()
        {
            try
            {
                var user = userManager.GetUserName(User);
                var Company = companyRepository.GetOne(where:e=>e.UserName==user).Id;
                var Hotels = hotelRepository.Get(where: e => e.CompanyId == Company);
                return View(Hotels.ToList());
            }
            catch (Exception ex)
            {
                
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: HotelsController/Details/5
        public ActionResult Details(int id)
        {
            var user = userManager.GetUserId(User);
            var Hotels = hotelRepository.Get([e => e.HotelAmenities, e => e.Rooms, e => e.ImageLists],where: e => e.CompanyId.ToString() == user && e.Id==id);
            return View(Hotels);
        }

        // GET: HotelsController/Create
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
                ViewBag.Img = userId;
                hotelRepository.CreateWithImage(hotel, ImgFile, "homeImage", "CoverImg");
                return RedirectToAction(nameof(Index));
            }

            return View(hotel); 
        }

        // GET: HotelsController/Edit/5
        public ActionResult Edit(int id)
        {
            var user = userManager.GetUserName(User);
            ViewBag.CompanyId = companyRepository.GetOne(where: e => e.UserName == user);
            var hotel = hotelRepository.GetOne(where: e => e.Id == id);
            return View(hotel);
        }

        // POST: HotelsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Hotel hotel, IFormFile ImgFile)
        {
            ModelState.Remove(nameof(ImgFile));
            if (ModelState.IsValid)
            {
                var user = userManager.GetUserName(User);
                var oldHotel = hotelRepository.GetOne(where: e => e.Id == hotel.Id);
                hotelRepository.UpdateImage(hotel, ImgFile, oldHotel.CoverImg, "homeImage", "CoverImg");
                return RedirectToAction(nameof(Index));
            }
            return NotFound();

        }

        // GET: HotelController/Delete/5
        public ActionResult Delete(int id)
        {
            var hotel = hotelRepository.GetOne(where: e => e.Id == id);

            return View(hotel);
        }

        // POST: HotelsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Hotel hotel)
        {
            var oldHotel = hotelRepository.GetOne(where: e => e.Id == hotel.Id);
            var user = userManager.GetUserName(User);
            hotelRepository.DeleteWithImage(oldHotel, "homeImage", oldHotel.CoverImg);
            hotelRepository.Commit();
            return RedirectToAction(nameof(Index));
        }
    }
}
