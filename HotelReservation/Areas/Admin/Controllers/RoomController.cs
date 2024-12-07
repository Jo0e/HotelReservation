using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Models.Models;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoomController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public RoomController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // GET: RoomController
        public ActionResult Index(int id)
        {

            IEnumerable<Room> rooms;
            if (id != 0)
            {
                Response.Cookies.Append("HotelIdCookie", id.ToString());
                rooms = unitOfWork.RoomRepository.Get(where: e => e.HotelId == id, include: [e => e.Hotel, w => w.RoomType]);
                ViewBag.HotelId = id;
                return View(rooms);
            }
            else if (id == 0)
            {
                var hotelId = int.Parse(Request.Cookies["HotelIdCookie"]);
                rooms = unitOfWork.RoomRepository.Get(where: e => e.HotelId == hotelId, include: [e => e.Hotel, w => w.RoomType]);
                ViewBag.HotelId = hotelId;
                return View(rooms);
            }
            return NotFound();

        }


        // GET: RoomController/Create
        public ActionResult Create(int hotelId)
        {
            //var hotelId = int.Parse(Request.Cookies["HotelId"]);
            ViewBag.HotelId = hotelId;
            ViewBag.Type = unitOfWork.RoomTypeRepository.Get(where: t=>t.HotelId==hotelId);
            return View();
        }

        // POST: RoomController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Room room)
        {
            //room.HotelId = hotelId;

            unitOfWork.RoomRepository.Create(room);
            unitOfWork.Complete();
            return RedirectToAction(nameof(Index));
        }

        
        // POST: RoomController/Edit/5

        public ActionResult Book(int id)
        {
            var room = unitOfWork.RoomRepository.GetOne(where: r => r.Id == id);
            if (room.IsAvailable == true)
            {
                room.IsAvailable = false;
            }
            else
            {
                room.IsAvailable = true;
            }
            unitOfWork.RoomRepository.Update(room);
            unitOfWork.Complete();
            return RedirectToAction(nameof(Index));
        }

        // GET: RoomController/Delete/5
        public ActionResult Delete(int id)
        {
            var room = unitOfWork.RoomRepository.GetOne(where: a => a.Id == id, include: [e => e.Hotel, w => w.RoomType]);
            return View(room);
        }

        // POST: RoomController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Room room)
        {
            unitOfWork.RoomRepository.Delete(room);
            unitOfWork.Complete();
            return RedirectToAction(nameof(Index));
        }
    }
}
