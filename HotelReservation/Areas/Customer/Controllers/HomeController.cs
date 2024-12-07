using System.Diagnostics;
using System.Linq;
using AutoMapper;
using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Utilities.Utility;


namespace HotelReservation.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IMapper mapper;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager, IMapper mapper)
        {
            _logger = logger;
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.mapper = mapper;
        }



        public IActionResult Index(string? search = null)
        {
            var hotels = unitOfWork.HotelRepository.Get([h => h.HotelAmenities, h => h.Rooms]);
            var hotelAmenities = unitOfWork.HotelAmenitiesRepository.Get([o => o.Amenity]);
            //var roomCondition = roomRepository.Get([n => n.IsAvailable]);
            var hotelAmenitiesResults = Enumerable.Empty<Hotel>();
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                hotels = hotels.Where(h =>
                h.City.Contains(search, StringComparison.OrdinalIgnoreCase));
            }


            // ViewBag.HotelAmenities = hotelAmenities;

            var hotelsByCity = hotels.GroupBy(h => h.City)
                               .Select(g => g.First())
                               .ToList();
            int TotalResult = hotels.Count();

            ViewBag.totalResult = TotalResult;
            ViewBag.search = search;

            return View(hotelsByCity);
        }



        public IActionResult HotelsByCity(string city, int? stars, List<string> amenitiess, string? search = null, int pageNumber = 1)
        {

            const int pageSize = 5;



            var hotels = unitOfWork.HotelRepository.Get([h => h.HotelAmenities, h => h.Rooms], where: c => c.City.Contains(city));
            //.Where(h => h.City.Equals(city, StringComparison.OrdinalIgnoreCase));

            //var hotels = unitOfWork.HotelRepository.Get([h => h.HotelAmenities, h => h.Rooms])
            //                             .Where(h => h.City.Equals(city, StringComparison.OrdinalIgnoreCase));
            var hotelAmenities = unitOfWork.HotelAmenitiesRepository.Get([o => o.Amenity]);
            var hotelAmenitiesResults = Enumerable.Empty<Hotel>();

            if (stars.HasValue)
            {
                hotels = hotels.Where(h => h.Stars == stars.Value);
            }
            else
            {
                RedirectToAction("NotFound");
            }


            if (!string.IsNullOrWhiteSpace(search))
            {

                search = search.Trim();
                hotels = hotels.Where(h => h.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || h.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
                || h.Address.Contains(search, StringComparison.OrdinalIgnoreCase) || h.City.Contains(search, StringComparison.OrdinalIgnoreCase));

                //|| h.HotelAmenities.Any(a => a.Amenity.Name.Contains(search, StringComparison.OrdinalIgnoreCase))).ToList();   /*Contains(search, StringComparison.OrdinalIgnoreCase) || h.Rooms.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));*/
            }


            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                var amenities = unitOfWork.HotelAmenitiesRepository.Get([ha => ha.Amenity])
                            .Where(ha => ha.Amenity.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                            .Select(ha => ha.Hotel)
                            .ToList();

                hotelAmenitiesResults = amenities;
            }
            hotels = hotels.Union(hotelAmenitiesResults).ToList();

            if (amenitiess != null && amenitiess.Count > 0)
            {
                hotels = hotels.Where(h => h.HotelAmenities.Any(ha => amenitiess.Contains(ha.Amenity.Name, StringComparer.OrdinalIgnoreCase)));
            }

            int TotalResult = hotels.Count();
            var paginatedHotels = hotels.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.City = city;
            ViewBag.Stars = stars;
            ViewBag.Amenities = amenitiess;
            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = (int)Math.Ceiling((double)TotalResult / pageSize);
            ViewBag.AllAmenities = unitOfWork.AmenityRepository.Get();
            ViewBag.totalResult = TotalResult;
            ViewBag.search = search;

            return View(paginatedHotels);
        }

        // Displays hotel details by ID
        public IActionResult Details(int id)
        {
            var hotel = unitOfWork.HotelRepository.GetOne(
                [h => h.Rooms, h => h.ImageLists, h => h.HotelAmenities, h => h.RoomTypes, h => h.Comments],
                where: h => h.Id == id
            );

            if (hotel != null)
            {
                ViewBag.Comment = unitOfWork.CommentRepository.Get
                    (where: h => h.HotelId == id, include: [u => u.User]);
                return View(hotel);
            }

            return RedirectToAction("NotFound");
        }



        public async Task<IActionResult> AddComment(int hotelId, string comment)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("NotFound");
            }
            var appUser = user as ApplicationUser;

            Comment NewComment = new Comment()
            {
                CommentString = comment,
                DateTime = DateTime.Now,
                HotelId = hotelId,
                UserId = user.Id,
            };
            unitOfWork.CommentRepository.Create(NewComment);
            unitOfWork.Complete();
            return RedirectToAction("Details", new { id = hotelId });
        }
        [HttpPost]
        public async Task<IActionResult> EditComment(int id, string commentString)
        {
            var comment = unitOfWork.CommentRepository.GetOne(where: p => p.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            comment.CommentString = commentString;
            comment.IsEdited = true;
            unitOfWork.Complete();

            return RedirectToAction("Details", new { id = comment.HotelId });
        }

        public async Task<IActionResult> LikeComment(int commentId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("NotFound");
            }
            var comment = unitOfWork.CommentRepository.GetOne(where: c => c.Id == commentId);

            if (comment == null) {
                return RedirectToAction("NotFound");
            }
            var isExist = comment.ReactionUsersId.Any(e => e.Equals(user.Id));
            if (isExist)
            {
                //comment.ReactionUsersId.Remove(user.Id);

                return RedirectToAction("Details", new { id = comment.HotelId });

            }
            comment.Likes++;
            comment.ReactionUsersId.Add(user.Id);
            unitOfWork.Complete();
            return RedirectToAction("Details", new { id = comment.HotelId });
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

