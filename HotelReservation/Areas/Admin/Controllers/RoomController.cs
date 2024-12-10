using AutoMapper;
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
        private readonly IMapper mapper;

        public RoomController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        // GET: RoomController
        public ActionResult Index(int id, string roomType = "All")
        {
            if (id != 0)
            {
                Response.Cookies.Append("HotelIdCookie", id.ToString());
            }
            else
            {
                id = int.Parse(Request.Cookies["HotelIdCookie"]);
            }

            var rooms = unitOfWork.RoomRepository.Get(
                where: e => e.HotelId == id && (roomType == "All" || e.RoomType.Type.ToString() == roomType),
                include: [e => e.Hotel, w => w.RoomType]
            );

            ViewBag.HotelId = id;
            ViewBag.RoomType = roomType;
            return View(rooms);
        }

        // GET: RoomController/Create
        public ActionResult Create(int hotelId)
        {
            //var hotelId = int.Parse(Request.Cookies["HotelId"]);
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


        // POST: RoomController/Edit/5

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
            return RedirectToAction(nameof(Index));
        }

        // GET: RoomController/Delete/5
        public ActionResult Delete(int id)
        {
            var room = unitOfWork.RoomRepository.GetOne(where: a => a.Id == id,
                include: [e => e.Hotel, w => w.RoomType]);
            if (room == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
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
