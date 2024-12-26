using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Utilities.Utility;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.AdminRole)]
    public class ReservationController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<ReservationController> logger;
        private readonly UserManager<IdentityUser> userManager;

        public ReservationController(IUnitOfWork unitOfWork, ILogger<ReservationController> logger,UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.userManager = userManager;
        }

        // GET: ReservationController/Index
        public ActionResult Index(int id)
        {
            if (id != 0)
            {
                Response.Cookies.Append("HotelIdCookie", id.ToString());
            }
            else
            {
                id = int.Parse(Request.Cookies["HotelIdCookie"]);
            }
            var reservations = unitOfWork.ReservationRepository.Get(where: e => e.HotelId == id, include: [e => e.User, e => e.Hotel])
                .OrderByDescending(a=>a.CreatedAt);
            ViewBag.HotelId = id;            
            return View(reservations);
        }


        //public IActionResult Edit(int id)
        //{
        //    var reservation = unitOfWork.ReservationRepository.GetOne(where: e => e.Id == id);
        //    return View(reservation);
        //}
        //[HttpPost]
        //public IActionResult Edit(Reservation reservation)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        unitOfWork.ReservationRepository.Update(reservation);
        //        unitOfWork.Complete();
        //        return RedirectToAction("Index");
        //    }
        //    return View(reservation);
        //}
        [HttpPost]
        public IActionResult Delete(int reservationId) 
        {
            var reservation = unitOfWork.ReservationRepository.GetOne(where: e => e.Id == reservationId, include: [e => e.Hotel, e => e.User]);
            var reservationRoom = unitOfWork.ReservationRoomRepository.GetOne(include: [e => e.Room], where: e => e.ReservationID == reservationId);
            if (reservation == null || reservationRoom == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }

            reservationRoom.Room.IsAvailable = true;
            unitOfWork.RoomRepository.Update(reservationRoom.Room);
            unitOfWork.ReservationRoomRepository.Delete(reservationRoom);
            unitOfWork.ReservationRepository.Delete(reservation);
            unitOfWork.CompleteAsync();

            Log("Cancel Reservation",nameof(reservation)+" "+ $"{reservation.Hotel.Name}");
            TempData["success"] = "Reservation deleted successfully.";

            return RedirectToAction("Index");
        }

        public async void Log(string action, string entity)
        {
            LoggerHelper.LogAdminAction(logger, User.Identity.Name, action, entity);

        }
    }
}
