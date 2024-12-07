using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using System.Linq.Expressions;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    public class RoomsController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public RoomsController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // GET: RoomsController
        public ActionResult Index(int id)
        {

            int resolvedHotelId = id;

            if (id == 0)
            {
                var hotelIdCookie = Request.Cookies["HotelIdCookie"];
                if (hotelIdCookie == null || !int.TryParse(hotelIdCookie, out resolvedHotelId))
                {
                    return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                }
            }
            else
            {
                Response.Cookies.Append("HotelIdCookie", id.ToString());
            }

            var rooms = unitOfWork.RoomRepository.Get(
                where: e => e.HotelId == resolvedHotelId,
                include:[e => e.Hotel,w => w.RoomType]);

            ViewBag.RoomsCount = rooms.Select(e => e.RoomType.AvailableRooms ?? 0).ToList();
            ViewBag.HotelId = resolvedHotelId;

            return View(rooms);

        }

        // GET: RoomsController/Details/5
        public ActionResult Details(int id)
        {
            var room = unitOfWork.RoomRepository.GetOne(
                [e => e.Hotel, w => w.RoomType],
                where: r => r.Id == id
            );

            if (room == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });

            }

            return View(room);
        }
        // GET: RoomsController/Create
        public ActionResult Create(int hotelId)
        {
           
            ViewBag.HotelId = hotelId;
            ViewBag.Type = unitOfWork.RoomTypeRepository.Get(where: t => t.HotelId == hotelId);
            return View();
        }

        // POST: RoomController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Room room)
        {
            if (!ModelState.IsValid)
            {
                unitOfWork.RoomRepository.Create(room);
                unitOfWork.Complete();
                return RedirectToAction(nameof(Index), new { id = room.HotelId });
            }
            return View(room);
        }
        // POST: RoomsController/Book/5
        public ActionResult Book(int id)
        {
            var room = unitOfWork.RoomRepository.GetOne(where: r => r.Id == id);
            if (room == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });

            }

            room.IsAvailable = !room.IsAvailable;

            unitOfWork.RoomRepository.Update(room);
            unitOfWork.Complete();

            return RedirectToAction(nameof(Index), new { id = room.HotelId });
        }

        public ActionResult Delete(int id)
        {
            var room = unitOfWork.RoomRepository.GetOne(
               [e => e.Hotel, w => w.RoomType],
                where: a => a.Id == id
            );

            if (room == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });

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
                unitOfWork.RoomRepository.Delete(room);
                 unitOfWork.Complete();

            return RedirectToAction(nameof(Index), new { id = room.HotelId });
        }
    }
}
