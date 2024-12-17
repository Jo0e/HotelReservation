using AutoMapper;
using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Stripe;
using Utilities.Utility;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoomController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ILogger<RoomController> logger;
        private readonly UserManager<IdentityUser> userManager;

        public RoomController(IUnitOfWork unitOfWork, IMapper mapper,ILogger<RoomController> logger,UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.logger = logger;
            this.userManager = userManager;
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
                TempData["success"] = $"Successfully created {count} room(s).";
                Log(nameof(Create), nameof(room) + " " + $"count: {count}");
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
            TempData["success"] = room.IsAvailable ? "Room is now available." : "Room is now unavailable.";
            Log(nameof(Book), nameof(room) + " " + $"Book: {room.IsAvailable}");
            return RedirectToAction(nameof(Index));
        }

        // GET: RoomController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    var room = unitOfWork.RoomRepository.GetOne(where: a => a.Id == id,
        //        include: [e => e.Hotel, w => w.RoomType]);
        //    if (room == null)
        //    {
        //        return RedirectToAction("NotFound", "Home", new { area = "Customer" });
        //    }
        //    return View(room);
        //}

        // POST: RoomController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Room room)
        {
            var toDelete = unitOfWork.RoomRepository.GetOne(where: e => e.Id == room.Id);
            if ( toDelete!= null )
            {
                unitOfWork.RoomRepository.Delete(toDelete);
                unitOfWork.CompleteAsync();
                TempData["success"] = "Room deleted successfully.";
                Log(nameof(Delete), nameof(room) + " " + $"Id: {room.Id}");
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("NotFound", "Home", new { area = "Customer" });

        }

        public async void Log(string action, string entity)
        {
            LoggerHelper.LogAdminAction(logger, User.Identity.Name, action, entity);

        }
    }
}
