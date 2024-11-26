using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Models;

namespace HotelReservation.Areas.Company.Controllers
{
    public class RoomsController : Controller
    {
        private readonly IRoomRepository roomRepository;
        private readonly IHotelRepository hotelRepository;
        private readonly IRepository<RoomType> roomTypeRepository;

        public RoomsController(IRoomRepository roomRepository,IHotelRepository hotelRepository,IRepository<RoomType> roomTypeRepository)
        {
            this.roomRepository = roomRepository;
            this.hotelRepository = hotelRepository;
            this.roomTypeRepository = roomTypeRepository;
        }
        // GET: RoomsController
        public ActionResult Index(int id)
        {
            var rooms = roomRepository.Get(
                [e=>e.Hotel],
                where: e => e.HotelId == id
            );

            if (rooms == null || !rooms.Any())
            {
                return NotFound("No rooms found for the specified hotel.");
            }

            ViewBag.HotelId = id; 
            return View(rooms);
        }


        // GET: RoomsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult Create(int id)
        {
            var hotel = hotelRepository.GetOne(where: e => e.Id == id);
            if (hotel == null)
            {
                return NotFound("Hotel not found.");
            }

            ViewBag.HotelId = hotel.Id;
            ViewBag.RoomTypes = roomTypeRepository.Get(); 
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Room room)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RoomTypes = roomTypeRepository.Get(); 
                return View(room);
            }

            roomRepository.Create(room);
            roomRepository.Commit();

            return RedirectToAction(nameof(Index), new { id = room.HotelId });
        }


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
        // GET: RoomsController/Delete/5
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


        // POST: RoomController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Room room)
        {
            if (room == null)
            {
                return NotFound("Room not found.");
            }

            try
            {
                roomRepository.Delete(room);
                roomRepository.Commit();
            }
            catch (Exception ex)
            {
                
                ModelState.AddModelError("", "Unable to delete room. It may be related to other records.");
                return View(room);
            }

            return RedirectToAction(nameof(Index), new { id = room.HotelId });
        }

    }
}
