using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
            try
            {
                IEnumerable<Room> rooms;
                if (id != 0)
                {
                    Response.Cookies.Append("HotelIdCookie", id.ToString());
                    rooms = unitOfWork.RoomRepository.Get(where: e => e.HotelId == id, include: [e => e.Hotel, w => w.RoomType]);
                    ViewBag.roomsCount = rooms.Select(e => e.RoomType.AvailableRooms.Value);
                    ViewBag.HotelId = id;
                    return View(rooms);
                }
                else if (id == 0)
                {
                    var hotelId = int.Parse(Request.Cookies["HotelIdCookie"]);
                    rooms = unitOfWork.RoomRepository.Get(where: e => e.HotelId == hotelId, include: [e => e.Hotel, w => w.RoomType]);
                    ViewBag.HotelId = hotelId;
                    return View(rooms);
                }
                return NotFound();
            }
            catch (Exception)
            {
                // Log the exception (not shown here)
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        // GET: RoomsController/Details/5
        public ActionResult Details(int id)
        {
            try
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
            catch (Exception)
            {
                // Log the exception (not shown here)
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        // GET: RoomsController/Create
        public ActionResult Create(int hotelId)
        {
            ViewBag.HotelId = hotelId;
            ViewBag.Type = unitOfWork.RoomTypeRepository.Get(where: t => t.HotelId == hotelId);
            return View();
        }

        // POST: RoomsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Room room)
        {
            if (!ModelState.IsValid)
            {
                return View(room); // Return the view with validation errors
            }

            try
            {
                unitOfWork.RoomRepository.Create(room);
                unitOfWork.Complete();
                return RedirectToAction(nameof(Index), new { id = room.HotelId });
            }
            catch (Exception)
            {
                // Log the exception (not shown here)
                ModelState.AddModelError(string.Empty, "An error occurred while creating the room.");
                return View(room);
            }
        }

        // POST: RoomsController/Book/5
        public ActionResult Book(int id)
        {
            try
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
            catch (Exception)
            {
                // Log the exception (not shown here)
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        public ActionResult Delete(int id)
        {
            try
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
            catch (Exception)
            {
                // Log the exception (not shown here)
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
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

            try
            {
                unitOfWork.RoomRepository.Delete(room);
                unitOfWork.Complete();
                return RedirectToAction(nameof(Index), new { id = room.HotelId });
            }
            catch (Exception)
            {
                // Log the exception (not shown here)
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the room.");
                return View(room);
            }
        }
    }
}