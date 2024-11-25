using System.Diagnostics;
using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models.Models;

namespace HotelReservation.Areas.Customer.Controllers
{

    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHotelRepository hotelRepository;

        public HomeController(ILogger<HomeController> logger, IHotelRepository hotelRepository)
        {
            _logger = logger;
            this.hotelRepository = hotelRepository;

        }


        public IActionResult Index(int page = 1, string? search = null)
        {

            if (page <= 0)
                page = 1;

            var hotels = hotelRepository.Get([h => h.HotelAmenities, h => h.Rooms]);

            if (search != null && search.Length > 0)
            {
                search = search.TrimStart();
                search = search.TrimEnd();
                hotels = hotels.Where(e => e.Name.Contains(search));
            }
            hotels = hotels.Skip((page - 1) * 5).Take(5);

            if (hotels.Any())
            {
                return View(hotels);
            }

            return RedirectToAction("NotFound");
        }




        public IActionResult Details(int id)
        {
            var hotels = hotelRepository.GetOne([h => h.Rooms, h => h.ImageLists, h => h.HotelAmenities], where: h => h.Id == id);
            if (hotels != null)
                return View(hotels);

            return RedirectToAction("NotFound");
        }


        public IActionResult NotFound()
        {
            return View();
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
