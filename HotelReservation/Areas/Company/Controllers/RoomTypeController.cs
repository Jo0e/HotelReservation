using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Models;
using Type = Models.Models.Type;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    public class RoomTypeController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public RoomTypeController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
       
        // GET: RoomType/Index
        public ActionResult Index(int hotelId)
        {
            int resolvedHotelId = hotelId;

            if (hotelId == 0)
            {
                var hotelIdCookie = Request.Cookies["HotelId"];
                if (hotelIdCookie == null || !int.TryParse(hotelIdCookie, out resolvedHotelId))
                {
                    return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                }
            }
            else
            {
                Response.Cookies.Append("HotelId", hotelId.ToString());
            }

            var types = unitOfWork.RoomTypeRepository.Get(where: a => a.HotelId == resolvedHotelId);

            ViewBag.HotelId = resolvedHotelId;

            return View(types);
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
            if (ModelState.IsValid)
            {
                unitOfWork.RoomTypeRepository.Create(roomType);
                unitOfWork.Complete();
                return RedirectToAction(nameof(Index));
            }
            return View(roomType);
        }

        // GET: RoomTypeController/Edit/5
        public ActionResult Edit(int id)
        {
            var roomType = unitOfWork.RoomTypeRepository.GetOne(where: e => e.Id == id);
            ViewBag.HotelId = roomType?.HotelId;
            if (roomType == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });


            }

            ViewBag.Types = new SelectList(Enum.GetValues(typeof(Type)));
            return View(roomType);
        }

        // POST: RoomTypeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RoomType roomType)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.RoomTypeRepository.Update(roomType);
                unitOfWork.Complete();
                return RedirectToAction(nameof(Index), new { hotelId = roomType.HotelId });
            }
            return View(roomType);
        }

        // POST: RoomTypeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                var roomType = unitOfWork.RoomTypeRepository.GetOne(where: e => e.Id == id);
                if (roomType == null)
                {
                    return RedirectToAction("NotFound", "Home", new { area = "Customer" });

                }

                unitOfWork.RoomTypeRepository.Delete(roomType);
                unitOfWork.Complete();
                return RedirectToAction(nameof(Index), new { hotelId = roomType.HotelId });
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
