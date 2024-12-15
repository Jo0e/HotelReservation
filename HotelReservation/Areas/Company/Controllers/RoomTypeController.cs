using HotelReservation.Areas.Admin.Controllers;
using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Models;
using Utilities.Utility;
using Type = Models.Models.Type;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.CompanyRole)]
    public class RoomTypeController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<RoomTypeController> logger;
        private readonly UserManager<IdentityUser> userManager;

        public RoomTypeController(IUnitOfWork unitOfWork, ILogger<RoomTypeController> logger, UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.userManager = userManager;
        }

        // GET: RoomType/Index
        public ActionResult Index(int hotelId)
        {
            IEnumerable<RoomType> types;
            if (hotelId != 0)
            {
                Response.Cookies.Append("HotelId", hotelId.ToString());
            }
            else
            {
                hotelId = int.Parse(Request.Cookies["HotelId"]);
            }
            var accesses = CheckAccesses(hotelId);
            if (!accesses)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            types = unitOfWork.RoomTypeRepository.Get(where: a => a.HotelId == hotelId);
            ViewBag.HotelId = hotelId;
            return View(types);
        }

        // GET: RoomTypeController/Create
        public ActionResult Create(int hotelId)
        {
            var accesses = CheckAccesses(hotelId);
            if (!accesses)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
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
                Log(nameof(Create), nameof(RoomType) + " " + $"{roomType.Type - roomType.PricePN}");
                unitOfWork.Complete();
                TempData["success"] = "Room type created successfully.";

                return RedirectToAction(nameof(Index));
            }
            return View(roomType);
        }

        // GET: RoomTypeController/Edit/5
        public ActionResult Edit(int id)
        {
            var roomType = unitOfWork.RoomTypeRepository.GetOne(where: e => e.Id == id);
            if (roomType == null)
            {
            return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            var accesses = CheckAccesses(roomType.HotelId);
            if (!accesses)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            ViewBag.HotelId = roomType?.HotelId;
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
                Log(nameof(Edit), nameof(RoomType) + " " + $"{roomType.Type - roomType.PricePN}");
                unitOfWork.RoomTypeRepository.Update(roomType);
                unitOfWork.Complete();
                TempData["success"] = "Room type updated successfully.";

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
                Log(nameof(Delete), $"RoomType {roomType.Type - roomType.PricePN}");
                TempData["success"] = "Room type deleted successfully.";

                return RedirectToAction(nameof(Index), new { hotelId = roomType.HotelId });
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }
        public async void Log(string action, string entity)
        {
            var user = await userManager.GetUserAsync(User);
            LoggerHelper.LogAdminAction(logger, user.Id, user.Email, action, entity);
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
