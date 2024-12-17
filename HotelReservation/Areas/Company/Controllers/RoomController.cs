using AutoMapper;
using HotelReservation.Areas.Admin.Controllers;
using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly ILogger<RoomsController> logger;
        private readonly UserManager<IdentityUser> userManager;

        public RoomsController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RoomsController> logger, UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.logger = logger;
            this.userManager = userManager;
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

            var accesses = CheckAccesses(id);
            if (!accesses)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
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
            var accesses = CheckAccesses(hotelId);
            if (!accesses)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
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
                TempData["success"] = $"Successfully created {count} room(s).";
                Log(nameof(Create), nameof(room) + " " + $"count: {count}");

                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }
        // POST: RoomsController/Book/5
        [HttpPost]
        public ActionResult Book(int roomId)
        {
            var room = unitOfWork.RoomRepository.GetOne(where: r => r.Id == roomId);
            if (room == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }

            room.IsAvailable = !room.IsAvailable;
            unitOfWork.RoomRepository.Update(room);
            unitOfWork.Complete();
            Log(nameof(Book), nameof(room));
            TempData["success"] = room.IsAvailable ? "Room is now available." : "Room is now unavailable.";


            return RedirectToAction(nameof(Index), new { id = room.HotelId });
        }

        //public ActionResult Delete(int id)
        //{
        //    var room = unitOfWork.RoomRepository.GetOne(
        //       [e => e.Hotel, w => w.RoomType],
        //        where: a => a.Id == id
        //    );

        //    if (room == null)
        //    {
        //        return RedirectToAction("NotFound", "Home", new { area = "Customer" });
        //    }

        //    return View(room);
        //}

        // POST: RoomsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var room = unitOfWork.RoomRepository.GetOne(
               where: a => a.Id == id );
            if (room == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            unitOfWork.RoomRepository.Delete(room);
            unitOfWork.Complete();
            Log(nameof(Delete), $"room Id: {room.Id}");
            TempData["success"] = "Room deleted successfully.";

            return RedirectToAction(nameof(Index), new { id = room.HotelId });
        }
        public async void Log(string action, string entity)
        {
            LoggerHelper.LogAdminAction(logger, User.Identity.Name, action, entity);

        }

        private bool CheckAccesses(int hotelId)
        {
            var user = userManager.GetUserName(User);
            var company = unitOfWork.CompanyRepository.GetOne(where: e => e.UserName == user, tracked: false);
            var hotel = unitOfWork.HotelRepository.GetOne(where: a => a.CompanyId == company.Id && a.Id == hotelId, tracked: false);
            return !(hotel == null);
        }
    }
}
