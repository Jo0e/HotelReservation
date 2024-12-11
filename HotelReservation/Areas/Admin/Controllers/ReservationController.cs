using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilities.Utility;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.AdminRole)]
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
            var reservations = unitOfWork.ReservationRepository.Get(where: e => e.HotelId == id, include: [e => e.User, e => e.Hotel]);
            return View(reservations);
        }
    }
}
