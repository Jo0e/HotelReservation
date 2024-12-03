using System.Diagnostics;
using System.Linq;
using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Utilities.Utility;

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
                hotels = hotels.Where(h=>
                h.City.Contains(search, StringComparison.OrdinalIgnoreCase)); 
            }


           // ViewBag.HotelAmenities = hotelAmenities;

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

            var hotels = hotelRepository.Get([h => h.HotelAmenities, h => h.Rooms])
                                         .Where(h => h.City.Equals(city, StringComparison.OrdinalIgnoreCase));
            var hotelAmenities = hotelAmenitiesRepository.Get([o => o.Amenity]);
            var hotelAmenitiesResults = Enumerable.Empty<Hotel>();



            if (string.IsNullOrWhiteSpace(city))
            {
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                hotels = hotels.Where(h => h.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || h.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
                || h.Address.Contains(search, StringComparison.OrdinalIgnoreCase) || h.City.Contains(search, StringComparison.OrdinalIgnoreCase)); //|| h.HotelAmenities.Any(a => a.Amenity.Name.Contains(search, StringComparison.OrdinalIgnoreCase))).ToList();   /*Contains(search, StringComparison.OrdinalIgnoreCase) || h.Rooms.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));*/
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                var amenities = hotelAmenitiesRepository.Get([ha => ha.Amenity])
                            .Where(ha => ha.Amenity.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                            .Select(ha => ha.Hotel)
                            .ToList();

                hotelAmenitiesResults = amenities;
            }
            hotels = hotels.Union(hotelAmenitiesResults).ToList();


            int TotalResult = hotels.Count();
            var paginatedHotels = hotels.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.city = city;
            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = (int)Math.Ceiling((double)TotalResult / pageSize);

            ViewBag.totalResult = TotalResult;
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
        [Authorize(Roles = SD.AdminRole)]
        public IActionResult AddLogo()
        {

            return View();
        }

        // POST: HotelController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.AdminRole)]
        public IActionResult AddLogo(IFormFile ImgFile)
        {
            if (ImgFile != null && ImgFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImgFile.FileName);
                var logoDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Logo");

                // Ensure the directory exists
                if (!Directory.Exists(logoDirectory))
                {
                    Directory.CreateDirectory(logoDirectory);
                }

                var filePath = Path.Combine(logoDirectory, fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    ImgFile.CopyTo(stream);
                }

                TempData["LogoPath"] = $"/images/Logo/{fileName}";
            }

            return RedirectToAction(nameof(AddLogo));
        }

        [Authorize(Roles = SD.AdminRole)]
        public IActionResult IndexLogo()
        {
            var logoDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Logo");
            string logoPath = "/images/Logo/default-logo.png";

            if (Directory.Exists(logoDirectory))
            {
                var files = Directory.GetFiles(logoDirectory);

                if (files.Length > 0)
                {
                    
                    var randomFile = files[new Random().Next(files.Length)];
                    logoPath = $"/images/Logo/{Path.GetFileName(randomFile)}";
                }
            }

            ViewBag.LogoPath = logoPath;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.AdminRole)]
        public IActionResult DeleteLogo()
        {
            try
            {

                var logoDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Logo");
                var files = Directory.GetFiles(logoDirectory);

                if (files.Length > 0)
                {
                    foreach (var file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }

                TempData["SuccessMessage"] = "Logo deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error occurred while deleting the logo: " + ex.Message;
            }

            return RedirectToAction("AddLogo");
        }



    }
}

