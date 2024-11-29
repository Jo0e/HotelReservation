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


        public IActionResult Index(string search = null)
        {
            var hotels = hotelRepository.Get([h => h.HotelAmenities, h => h.Rooms]);


          
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                hotels = hotels.Where(h => h.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                            h.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
            }
            var hotelsByCity = hotels.GroupBy(h => h.City)
                               .Select(g => g.First())
                               .ToList();
            int TotalResult = hotels.Count();

            ViewBag.totalResult = TotalResult;
            ViewBag.search = search;
            return View(hotelsByCity);
        }

        public IActionResult HotelsByCity(string city, string search = null, int pageNumber = 1)
        {
            const int pageSize = 5; 

            if (string.IsNullOrWhiteSpace(city))
            {
                return RedirectToAction("Index");
            }
            var hotels = hotelRepository.Get([h => h.HotelAmenities, h => h.Rooms])
                                        .Where(h => h.City.Equals(city, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                hotels = hotels.Where(h => h.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                            h.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            int totalHotels = hotels.Count(); 
            var paginatedHotels = hotels.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.city = city;
            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = (int)Math.Ceiling((double)totalHotels / pageSize);
            ViewBag.search = search;

            return View(paginatedHotels);
        }


        // Displays hotel details by ID
        public IActionResult Details(int id)
        {
            var hotel = hotelRepository.GetOne(
                [h => h.Rooms, h => h.ImageLists, h => h.HotelAmenities, h => h.RoomTypes] ,
                where: h => h.Id == id
            );

            if (hotel != null)
            {
                return View(hotel);
            }

            return RedirectToAction("NotFound");
        }

        // Privacy page (static content)
        public IActionResult Privacy()
        {
            return View();
        }

        // Custom not found page
        public IActionResult NotFound()
        {
            return View();
        }

        // Error page for unhandled exceptions
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
