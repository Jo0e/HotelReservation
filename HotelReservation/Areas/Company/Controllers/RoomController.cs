using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models.Models;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    public class RoomsController : Controller
    {
        private readonly IRoomTypeRepository typeRepository;
        private readonly IRoomRepository roomRepository;
        private readonly IHotelRepository hotelRepository;
        private readonly IRepository<RoomType> roomTypeRepository;

        public RoomsController(IRoomTypeRepository typeRepository,IRoomRepository roomRepository, IHotelRepository hotelRepository, IRepository<RoomType> roomTypeRepository)
        {
            this.typeRepository = typeRepository;
            this.roomRepository = roomRepository;
            this.hotelRepository = hotelRepository;
            this.roomTypeRepository = roomTypeRepository;
        }

        // GET: RoomsController
        public ActionResult Index(int id)
        {

            IEnumerable<Room> rooms;
            if (id != 0)
            {
                Response.Cookies.Append("HotelIdCookie", id.ToString());
                rooms = roomRepository.Get(where: e => e.HotelId == id, include: [e => e.Hotel, w => w.RoomType]);
                ViewBag.roomsCount = rooms.Select(e => e.RoomType.AvailableRooms.Value);
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

        // GET: RoomsController/Details/5
        public ActionResult Details(int id)
        {
            var room = roomRepository.GetOne(
                [e => e.Hotel, w => w.RoomType],
                where: r => r.Id == id
            );

            if (room == null)
            {
                return NotFound("Room not found.");
            }

            return View(room);
        }
        // GET: RoomsController/Create
        public ActionResult Create(int hotelId)
        {
           
            ViewBag.HotelId = hotelId;
            ViewBag.Type = typeRepository.Get();
            return View();
        }

        // POST: RoomController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Room room)
        {
            roomRepository.Create(room);
            roomRepository.Commit();
            return RedirectToAction(nameof(Index), new {id=room.HotelId});
        }
        // POST: RoomsController/Book/5
        public ActionResult Book(int id)
        {
            var room = roomRepository.GetOne(where: r => r.Id == id);
            if (room == null)
            {
                return NotFound("Room not found.");
            }

            room.IsAvailable = !room.IsAvailable;

            roomRepository.Update(room);
            roomRepository.Commit();

            return RedirectToAction(nameof(Index), new { id = room.HotelId });
        }

        public ActionResult Delete(int id)
        {
            var room = roomRepository.GetOne(
               [e => e.Hotel, w => w.RoomType],
                where: a => a.Id == id
            );

            if (room == null)
            {
                return NotFound("Room not found.");
            }

            return View(room);
        }

        // POST: RoomsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Room room)
        {
            if (room == null)
            {
                return NotFound("Room not found.");
            }
                roomRepository.Delete(room);
                 roomRepository.Commit();

            return RedirectToAction(nameof(Index), new { id = room.HotelId });
        }
    }
}
