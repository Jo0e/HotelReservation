using AutoMapper;
using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Utilities.Utility;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.CompanyRole)]
    public class RoomsController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public RoomsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        // GET: RoomsController
        public ActionResult Index(int id, string roomType = "All")
        {
            IEnumerable<Room> rooms;

            if (id != 0)
            {
                Response.Cookies.Append("HotelIdCookie", id.ToString());
            }
            else
            {
                id = int.Parse(Request.Cookies["HotelIdCookie"]);
            }

            rooms = unitOfWork.RoomRepository.Get(
                where: e => e.HotelId == id && (roomType == "All" || e.RoomType.Type.ToString() == roomType),
                include: [e => e.Hotel, w => w.RoomType]
            );

            ViewBag.HotelId = id;
            ViewBag.RoomType = roomType;
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
        public ActionResult Create(Room room, int count)
        {
            if (ModelState.IsValid)
            {
                var rooms = new List<Room>();
                for (int i = 0; i < count; i++)
                {
                    var newRoom = mapper.Map<Room>(room);
                    rooms.Add(newRoom);
                }
                unitOfWork.RoomRepository.AddRange(rooms);
                unitOfWork.Complete();
                return RedirectToAction(nameof(Index));
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
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            unitOfWork.RoomRepository.Delete(room);
            unitOfWork.Complete();

            return RedirectToAction(nameof(Index), new { id = room.HotelId });
        }
    }
}