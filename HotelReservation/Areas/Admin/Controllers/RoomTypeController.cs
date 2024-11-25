using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Models;
using Type = Models.Models.Type;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoomTypeController : Controller
    {
        private readonly IRoomTypeRepository typeRepository;
        private readonly IRoomRepository roomRepository;

        public RoomTypeController(IRoomTypeRepository TypeRepository, IRoomRepository roomRepository)
        {
            typeRepository = TypeRepository;
            this.roomRepository = roomRepository;
        }

        // GET: RoomTypeController
        public ActionResult Index(int hotelId)
        {
            IEnumerable<RoomType> types;
            if (hotelId != 0)
            {
                Response.Cookies.Append("HotelId", hotelId.ToString());
                types = typeRepository.Get(where: a => a.HotelId == hotelId);
                ViewBag.HotelId = hotelId;
                return View(types);
            }
            else if (hotelId == 0)
            {
                var Id = int.Parse(Request.Cookies["HotelId"]);
                types = typeRepository.Get(where: a => a.HotelId == Id);
                ViewBag.HotelId = Id;
                return View(types);
            }
            return NotFound();

        }

        // GET: RoomTypeController/Create
        public ActionResult Create(int hotelId)
        {
            ViewBag.HotelId = hotelId;
            ViewBag.Types = new SelectList(Enum.GetValues(typeof(Type)));
            return View();
        }

        // POST: RoomTypeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RoomType roomType)
        {
            typeRepository.Create(roomType);
            typeRepository.Commit();
            return RedirectToAction(nameof(Index));
        }

        // GET: RoomTypeController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: RoomTypeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: RoomTypeController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: RoomTypeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
