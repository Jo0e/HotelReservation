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
                include: [e => e.Hotel, w => w.RoomType]);

            ViewBag.RoomsCount = rooms.Select(e => e.RoomType.AvailableRooms ?? 0).ToList();
            ViewBag.HotelId = resolvedHotelId;

            return View(rooms);
        }


        // GET: RoomController/Create
        public ActionResult Create(int hotelId)
        {
            //var hotelId = int.Parse(Request.Cookies["HotelId"]);
            ViewBag.HotelId = hotelId;
            ViewBag.Type = unitOfWork.RoomTypeRepository.Get();
            return View();
        }

        // POST: RoomController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Room room)
        {
            //room.HotelId = hotelId;

            if (!ModelState.IsValid)
            {
                unitOfWork.RoomRepository.Create(room);
                unitOfWork.Complete();
                return RedirectToAction(nameof(Index), new { id = room.HotelId });
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

            return RedirectToAction(nameof(Index), new { id = room.HotelId });
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
