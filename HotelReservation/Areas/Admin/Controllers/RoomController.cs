using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models.Models;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoomController : Controller
    {
        private readonly IRoomRepository roomRepository;
        private readonly IHotelRepository hotelRepository;
        private readonly IRoomTypeRepository typeRepository;

        public RoomController(IRoomRepository roomRepository, IHotelRepository hotelRepository, IRoomTypeRepository TypeRepository)
        {
            this.roomRepository = roomRepository;
            this.hotelRepository = hotelRepository;
            this.typeRepository = TypeRepository;
        }

        // GET: RoomController
        public ActionResult Index(int id)
        {

            IEnumerable<Room> rooms;
            if (id != 0)
            {
                Response.Cookies.Append("HotelIdCookie", id.ToString());
                rooms = roomRepository.Get(where: e => e.HotelId == id, include: [e => e.Hotel, w => w.RoomType]);
                ViewBag.HotelId = id;
                return View(rooms);
            }
            else if (id == 0)
            {
                var hotelId = int.Parse(Request.Cookies["HotelIdCookie"]);
                rooms = roomRepository.Get(where: e => e.HotelId == hotelId, include: [e => e.Hotel, w => w.RoomType]);
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
            ViewBag.Type = typeRepository.Get();
            return View();
        }

        // POST: RoomController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Room room)
        {
            //room.HotelId = hotelId;

            roomRepository.Create(room);
            roomRepository.Commit();
            return RedirectToAction(nameof(Index));
        }

        
        // POST: RoomController/Edit/5

        public ActionResult Book(int id)
        {
            var room = roomRepository.GetOne(where: r => r.Id == id);
            if (room.IsAvailable == true)
            {
                room.IsAvailable = false;
            }
            else
            {
                room.IsAvailable = true;
            }
            roomRepository.Update(room);
            roomRepository.Commit();
            return RedirectToAction(nameof(Index));
        }

        // GET: RoomController/Delete/5
        public ActionResult Delete(int id)
        {
            var room = roomRepository.GetOne(where: a => a.Id == id, include: [e => e.Hotel, w => w.RoomType]);
            return View(room);
        }

        // POST: RoomController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Room room)
        {
            roomRepository.Delete(room);
            roomRepository.Commit();
            return RedirectToAction(nameof(Index));
        }
    }
}
