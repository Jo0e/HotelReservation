using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using System.Linq;
using Utilities.Utility;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles =SD.CompanyRole)]
    public class ReservationController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public ReservationController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        
        // GET: ReservationController/Index
        public ActionResult Index(int id)
        {
            var reservations = unitOfWork.ReservationRepository.Get(where: e => e.HotelId == id, include:[ e => e.User, e => e.Hotel ]);
            return View(reservations);
        }

        // GET: ReservationController/Edit/5
        public ActionResult Edit(int id)
        {
            var reservation = unitOfWork.ReservationRepository.GetOne(where: e => e.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }
            ViewBag.Hotels = unitOfWork.HotelRepository.Get().Select(h => new { h.Id, h.Name });
            return View(reservation);
        }

        // POST: ReservationController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    unitOfWork.ReservationRepository.Update(reservation);
                    unitOfWork.Complete();
                    TempData["success"] = "Reservation Updated successfully.";
                    return RedirectToAction(nameof(Index), new { id = reservation.HotelId });
                }
                catch
                {
                    ModelState.AddModelError("", "An error occurred while updating the reservation.");
                }
            }
            ViewBag.Hotels = unitOfWork.HotelRepository.Get().Select(h => new { h.Id, h.Name });
            return View(reservation);
        }

        // GET: ReservationController/Delete/5
        public ActionResult Delete(int id)
        {
            var reservation = unitOfWork.ReservationRepository.GetOne(where: e => e.Id == id , include: [e => e.Hotel, e => e.User]);
            var reservationRoom = unitOfWork.ReservationRoomRepository.GetOne(where: e => e.ReservationID == id);
            if (reservation == null || reservationRoom ==null)
            {
                return NotFound();
            }
            return View(reservation);
        }

        // POST: ReservationController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var reservation = unitOfWork.ReservationRepository.GetOne(where: e => e.Id == id, include: [e=>e.Hotel,e=>e.User]);
            var reservationRoom = unitOfWork.ReservationRoomRepository.GetOne(where: e => e.ReservationID == id);
            if (reservation == null || reservationRoom == null)
            {
                return NotFound();
            }
            unitOfWork.ReservationRoomRepository.Delete(reservationRoom);
            unitOfWork.Complete();
            unitOfWork.ReservationRepository.Delete(reservation);
            unitOfWork.Complete();
            TempData["success"] = "Reservation deleted successfully.";
            return RedirectToAction(nameof(Index), new { id = reservation.HotelId });
        }
    }
}
