
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
                var user = userManager.GetUserId(User);
                var Hotels = hotelRepository.Get(where: e => e.CompanyId.ToString() == user);
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
            var user = userManager.GetUserId(User);
            var Company = companyRepository.GetOne(where:e=>e.Id.ToString()==user);
            ViewData["CompanyId"] = Company?.Id;
            return View(Company);
        }

        // POST: HotelsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Hotel hotel, IFormFile ImgFile)
        {
            ModelState.Remove(nameof(ImgFile));
            if (ModelState.IsValid)
            {
                var userId = userManager.GetUserId(User);
                var company = companyRepository.GetOne(where: e => e.Id.ToString() == userId);

                if (company == null)
                {
                    ModelState.AddModelError("", "Invalid Company.");
                    return View(hotel);
                }

                hotel.CompanyId = company.Id;

                
                hotelRepository.CreateWithImage(hotel, ImgFile, "Main imgs", "CoverImg");
                return RedirectToAction(nameof(Index));
            }

            return View(hotel); 
        }

        // GET: HotelsController/Edit/5
        public ActionResult Edit(int id)
        {
            var user = userManager.GetUserId(User);
            ViewBag.CompanyId = companyRepository.Get(where: e => e.Id.ToString() == user);
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
                var oldHotel = hotelRepository.GetOne(where: e => e.Id == hotel.Id);
                hotelRepository.UpdateImage(hotel, ImgFile, oldHotel.CoverImg, "Main imgs", "CoverImg");
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
            hotelRepository.DeleteWithImage(oldHotel, "Main imgs", oldHotel.CoverImg);
            hotelRepository.Commit();
            return RedirectToAction(nameof(Index));
        }
    }
}
