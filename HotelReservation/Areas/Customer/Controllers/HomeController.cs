using System.Diagnostics;
using System.Linq;
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
        private readonly IHotelAmenitiesRepository hotelAmenitiesRepository;
        private readonly IRoomRepository roomRepository;
        public HomeController(ILogger<HomeController> logger, IHotelRepository hotelRepository,IHotelAmenitiesRepository hotelAmenitiesRepository,IRoomRepository roomRepository)
        {
            _logger = logger;
            this.hotelRepository = hotelRepository;
            this.hotelAmenitiesRepository = hotelAmenitiesRepository;
            this.roomRepository = roomRepository;
        }


        public IActionResult Index(string search = null)
        {
            var hotels = hotelRepository.Get([h => h.HotelAmenities, h => h.Rooms]);
            var hotelAmenities = hotelAmenitiesRepository.Get([o => o.Amenity]);
            //var roomCondition = roomRepository.Get([n => n.IsAvailable]);
            var hotelAmenitiesResults = Enumerable.Empty<Hotel>();
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                hotels = hotels.Where(h => h.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || h.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
                || h.Address.Contains(search, StringComparison.OrdinalIgnoreCase) || h.City.Contains(search, StringComparison.OrdinalIgnoreCase)); //|| h.HotelAmenities.Any(a => a.Amenity.Name.Contains(search, StringComparison.OrdinalIgnoreCase))).ToList();   /*Contains(search, StringComparison.OrdinalIgnoreCase) || h.Rooms.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));*/
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search=search.Trim();
                var amenities = hotelAmenitiesRepository.Get([ha => ha.Amenity])
                            .Where(ha => ha.Amenity.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                            .Select(ha => ha.Hotel)
                            .ToList();

                hotelAmenitiesResults = amenities;
            }

           // ViewBag.HotelAmenities = hotelAmenities;
            hotels = hotels.Union(hotelAmenitiesResults).ToList();

            var hotelsByCity = hotels.GroupBy(h => h.City)
                               .Select(g => g.First() )
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
