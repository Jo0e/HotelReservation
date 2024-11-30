using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Models;
using Type = Models.Models.Type;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    public class RoomTypeController : Controller
    {
        private readonly IRoomTypeRepository typeRepository;
        private readonly IHotelRepository hotelRepository;

        public RoomTypeController(IRoomTypeRepository typeRepository, IHotelRepository hotelRepository)
        {
            this.typeRepository = typeRepository;
            this.hotelRepository = hotelRepository;
        }

        // GET: RoomType/Index
        public ActionResult Index(int hotelId)
        {
            IEnumerable<RoomType> types;
            if (hotelId != 0)
            {
                Response.Cookies.Append("HotelId", hotelId.ToString());
                types = typeRepository.Get(where: a => a.HotelId == hotelId);
                ViewBag.HotelId = hotelId;
                return View(types);
            }
            else if (hotelId == 0)
            {
                var Id = int.Parse(Request.Cookies["HotelId"]);
                types = typeRepository.Get(where: a => a.HotelId == Id);
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
            typeRepository.Create(roomType);
            typeRepository.Commit();
            return RedirectToAction(nameof(Index));
        }

        // GET: RoomTypeController/Edit/5
        public ActionResult Edit(int id)
        {
            var roomType = typeRepository.GetOne(where: e => e.Id == id);
            ViewBag.HotelId = roomType?.HotelId;
            if (roomType == null)
            {
                return NotFound();
            }

            ViewBag.Types = new SelectList(Enum.GetValues(typeof(Type)));
            return View(roomType);
        }

        // POST: RoomTypeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RoomType roomType)
        {
                typeRepository.Update(roomType);
                typeRepository.Commit();
                return RedirectToAction(nameof(Index), new { hotelId = roomType.HotelId });
           
        }

        // POST: RoomTypeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                var roomType = typeRepository.GetOne(where: e => e.Id == id);
                if (roomType == null)
                {
                    return NotFound();
                }

                typeRepository.Delete(roomType);
                typeRepository.Commit();
                return RedirectToAction(nameof(Index), new { hotelId = roomType.HotelId });
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
