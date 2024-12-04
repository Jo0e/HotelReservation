using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Models;
using Type = Models.Models.Type;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoomTypeController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public RoomTypeController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // GET: RoomTypeController
        public ActionResult Index(int hotelId)
        {
            IEnumerable<RoomType> types;
            if (hotelId != 0)
            {
                Response.Cookies.Append("HotelId", hotelId.ToString());
                types = unitOfWork.RoomTypeRepository.Get(where: a => a.HotelId == hotelId);
                ViewBag.HotelId = hotelId;
                return View(types);
            }
            else if (hotelId == 0)
            {
                var Id = int.Parse(Request.Cookies["HotelId"]);
                types = unitOfWork.RoomTypeRepository.Get(where: a => a.HotelId == Id);
                ViewBag.HotelId = Id;
                return View(types);
            }
            return NotFound();

        }

        // GET: RoomTypeController/Create
        public ActionResult Create(int hotelId)
        {
            ViewBag.HotelId = hotelId;
            ViewBag.Types = new SelectList(Enum.GetValues(typeof(Type)));
            return View();
        }

        // POST: RoomTypeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RoomType roomType)
        {
            unitOfWork.RoomTypeRepository.Create(roomType);
            unitOfWork.Complete();
            return RedirectToAction(nameof(Index));
        }

        // GET: RoomTypeController/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.Types = new SelectList(Enum.GetValues(typeof(Type)));
            var type = unitOfWork.RoomTypeRepository.GetOne(where: e => e.Id == id);
            return View(type);
        }

        // POST: RoomTypeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RoomType roomType)
        {
            unitOfWork.RoomTypeRepository.Update(roomType);
            unitOfWork.Complete();
            return RedirectToAction(nameof(Index));
        }

        // GET: RoomTypeController/Delete/5
        public ActionResult Delete(int id)
        {
            var type = unitOfWork.RoomTypeRepository.GetOne(where: e => e.Id == id);
            unitOfWork.RoomTypeRepository.Delete(type);
            unitOfWork.Complete();
            return RedirectToAction(nameof(Index));
        }

        //// POST: RoomTypeController/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
