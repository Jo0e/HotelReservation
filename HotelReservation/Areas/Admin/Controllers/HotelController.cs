
using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Models;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HotelController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public HotelController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // GET: HotelController
        public ActionResult Index(string? search, int pageNumber = 1)
        {
            const int pageSize = 5;

            var hotel = unitOfWork.HotelRepository.Get(include: [p => p.company]);
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                hotel = hotel.Where(c =>
                    c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Address.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.City.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.company.UserName.Contains(search, StringComparison.OrdinalIgnoreCase));
            }
            var totalItems = hotel.Count();
            var hotels = hotel
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.SearchText = search;

            return View(hotels);
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
            if (ModelState.IsValid)
            {
                var oldHotel = unitOfWork.HotelRepository.GetOne(where: e => e.Id == hotel.Id);
                if (oldHotel == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                unitOfWork.HotelRepository.UpdateImage(hotel, ImgFile, oldHotel.CoverImg, "homeImage", "CoverImg");
                return RedirectToAction(nameof(Index));
            }
            return NotFound();

        }

        // GET: HotelController/Delete/5
        public ActionResult Delete(int id)
        {
            var hotel = unitOfWork.HotelRepository.GetOne(where: e => e.Id == id, include: [c => c.company]);

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
            unitOfWork.HotelRepository.Commit();
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
            return RedirectToAction(nameof(ImageList));
        }

        public ActionResult DeleteImgList(int id)
        {
            var img = unitOfWork.ImageListRepository.GetOne(where: e => e.Id == id , tracked:false);
            if (img == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            var hotel = unitOfWork.HotelRepository.GetOne(where: e => e.Id == img.HotelId , tracked: false);
            if (hotel == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            unitOfWork.ImageListRepository.DeleteImageList(id,hotel.Name);
            return RedirectToAction(nameof(ImageList));
        }

        public ActionResult DeleteAllImgList(int hotelId) 
        {
            var hotel = unitOfWork.HotelRepository.GetOne(include: [e=>e.ImageLists],where: e => e.Id == hotelId, tracked: false);
            if (hotel == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            unitOfWork.ImageListRepository.DeleteHotelFolder(hotel.ImageLists,hotel.Name);
            return RedirectToAction(nameof(ImageList));
        }
    }
}
