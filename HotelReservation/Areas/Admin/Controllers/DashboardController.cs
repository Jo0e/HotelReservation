using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var contactUs = unitOfWork.ContactUsRepository.Get(where: e => e.IsReadied == false);
            ViewBag.ContactUs = contactUs.Count();  
            return View();
        }
    }
}
