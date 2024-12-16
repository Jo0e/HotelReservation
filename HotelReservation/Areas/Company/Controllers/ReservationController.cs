using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using System.Linq;
using Utilities.Utility;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.CompanyRole)]
    public class ReservationController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<ReservationController> logger;
        private readonly UserManager<IdentityUser> userManager;

        public ReservationController(IUnitOfWork unitOfWork, ILogger<ReservationController> logger, UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.userManager = userManager;
        }

        // GET: ReservationController/Index
        public ActionResult Index(int id)
        {
            if (CheckAccesses(id))
            {
                var reservations = unitOfWork.ReservationRepository.Get(where: e => e.HotelId == id, include: [e => e.User, e => e.Hotel]);
                return View(reservations);
            }
            return RedirectToAction("NotFound", "Home", new { area = "Customer" });
        }

        // GET: ReservationController/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    var reservation = unitOfWork.ReservationRepository.GetOne(where: e => e.Id == id);
        //    if (reservation == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewBag.Hotels = unitOfWork.HotelRepository.Get().Select(h => new { h.Id, h.Name });
        //    return View(reservation);
        //}

        //// POST: ReservationController/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, Reservation reservation)
        //{
        //    if (id != reservation.Id)
        //    {
        //        return BadRequest();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            unitOfWork.ReservationRepository.Update(reservation);
        //            Log(nameof(Edit), nameof(reservation) + " " + $"{reservation.Hotel.Name}");
        //            unitOfWork.Complete();
        //            TempData["success"] = "Reservation Updated successfully.";
        //            return RedirectToAction(nameof(Index), new { id = reservation.HotelId });
        //        }
        //        catch
        //        {
        //            ModelState.AddModelError("", "An error occurred while updating the reservation.");
        //        }
        //    }
        //    ViewBag.Hotels = unitOfWork.HotelRepository.Get().Select(h => new { h.Id, h.Name });
        //    return View(reservation);
        //}

        // GET: ReservationController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    var reservation = unitOfWork.ReservationRepository.GetOne(where: e => e.Id == id , include: [e => e.Hotel, e => e.User]);
        //    var reservationRoom = unitOfWork.ReservationRoomRepository.GetOne(where: e => e.ReservationID == id);
        //    if (reservation == null || reservationRoom ==null)
        //    {
        //        return NotFound();
        //    }
        //    return View(reservation);
        //}

        // POST: ReservationController/Delete/5
        [HttpPost]
        public ActionResult Delete(int reservationId)
        {
            var reservation = unitOfWork.ReservationRepository.GetOne(where: e => e.Id == reservationId, include: [e => e.Hotel, e => e.User]);
            var reservationRoom = unitOfWork.ReservationRoomRepository.GetOne(include: [e => e.Room], where: e => e.ReservationID == reservationId);
            if (reservation == null || reservationRoom == null)
            {
                return NotFound();
            }
            reservationRoom.Room.IsAvailable = true;
            unitOfWork.RoomRepository.Update(reservationRoom.Room);
            unitOfWork.ReservationRoomRepository.Delete(reservationRoom);
            unitOfWork.ReservationRepository.Delete(reservation);
            unitOfWork.Complete();
            Log("Cancel Reservation", nameof(reservation) + " " + $"{reservation.Hotel.Name}");
            TempData["success"] = "Reservation deleted successfully.";
            return RedirectToAction(nameof(Index), new { id = reservation.HotelId });
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
