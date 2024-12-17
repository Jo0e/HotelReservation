using System.Diagnostics;
using System.Linq;
using AutoMapper;
using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using NuGet.Configuration;
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
            var hotels = unitOfWork.HotelRepository.Get();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                hotels = hotels.Where(h =>
                h.City.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var hotelsByCity = hotels.GroupBy(h => h.City)
                               .Select(g => g.First())
                               .ToList();

            var hotelCounts = hotels.GroupBy(e => e.City)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.HotelCounts = hotelCounts;
            ViewBag.search = search;

            return View(hotelsByCity);
        }



        public IActionResult HotelsByCity(string city, List<int>? stars, List<string> amenities, string? search = null, int pageNumber = 1)
        {
            const int pageSize = 6;
            IEnumerable<Hotel> hotels = Enumerable.Empty<Hotel>();
            if (string.IsNullOrWhiteSpace(search))
            {
                hotels = unitOfWork.HotelRepository.HotelsWithCity(city);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                hotels = unitOfWork.HotelRepository.HotelsWithCity(city)
                    .Where(h => h.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || h.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || h.Address.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || h.HotelAmenities.Any(ha => ha.Amenity.Name.Contains(search, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            if (stars != null && stars.Count > 0)
            {
                hotels = hotels.Where(h => stars.Contains(h.Stars));
            }
            if (amenities != null && amenities.Count > 0)
            {
                hotels = hotels.Where(h => h.HotelAmenities.Any(ha => amenities.Contains(ha.Amenity.Name, StringComparer.OrdinalIgnoreCase)));
            }

            int TotalResult = hotels.Count();
            var paginatedHotels = hotels.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.City = city;
            ViewBag.Stars = stars;
            ViewBag.Amenities = amenities;
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
            var hotel = unitOfWork.HotelRepository.HotelDetails(id);
            if (hotel != null)
            {
                ViewBag.Rate = CalculateAverageRating(id);
                ViewBag.Comment = unitOfWork.CommentRepository.Get
                    (where: h => h.HotelId == id, include: [u => u.User]);
                return View(hotel);
            }

            return RedirectToAction("NotFound", "Home");
        }

        [Authorize]
        public async Task<IActionResult> AddComment(int hotelId, string comment)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            Comment NewComment = new()
            {
                CommentString = comment,
                DateTime = DateTime.Now,
                HotelId = hotelId,
                UserId = user.Id,
            };
            unitOfWork.CommentRepository.Create(NewComment);
            TempData["success"] = "Comment added successfully!";
            unitOfWork.Complete();
            return RedirectToAction("Details", new { id = hotelId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditComment(int id, string commentString)
        {
            var comment = unitOfWork.CommentRepository.GetOne(where: p => p.Id == id);
            var user = await userManager.GetUserAsync(User);
            if (comment == null || user == null || comment.UserId != user.Id)
            {
                return RedirectToAction("NotFound", "Home");
            }

            comment.CommentString = commentString;
            comment.IsEdited = true;
            unitOfWork.Complete();
            TempData["success"] = "Comment edited successfully!";
            return RedirectToAction("Details", new { id = comment.HotelId });
        }
        [Authorize]
        public async Task<IActionResult> LikeComment(int commentId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            var comment = unitOfWork.CommentRepository.GetOne(where: c => c.Id == commentId);

            if (comment == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            var isExist = comment.ReactionUsersId.Any(e => e.Equals(user.Id));
            if (isExist)
            {
                comment.ReactionUsersId.Remove(user.Id);
                comment.Likes--;
                unitOfWork.Complete();
                return RedirectToAction("Details", new { id = comment.HotelId });

            }
            comment.Likes++;
            comment.ReactionUsersId.Add(user.Id);
            unitOfWork.Complete();
            return RedirectToAction("Details", new { id = comment.HotelId });
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportComment(int commentId, string UserRequestString)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            var contactUs = GetContactUs(user.Id, user.Email, ContactUs.RequestType.Complaint, UserRequestString, commentId);
            //var contactUs = new ContactUs
            //{
            //    UserId = user.Id,
            //    Name = user.Email,
            //    Request = ContactUs.RequestType.Complaint,
            //    UserRequestString = $"Comment Report: \r\n{UserRequestString}",
            //    HelperId = commentId,
            //};
            unitOfWork.ContactUsRepository.Create(contactUs);
            unitOfWork.Complete();
            TempData["success"] = "Your comment report has been submitted successfully.";
            return RedirectToAction("Index");

        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            var toDelete = unitOfWork.CommentRepository.GetOne(where: e => e.Id == commentId);
            if (toDelete != null&&toDelete.UserId==user.Id || toDelete != null&&User.IsInRole(SD.AdminRole))
            {
                unitOfWork.CommentRepository.Delete(toDelete);
                unitOfWork.Complete();
                TempData["success"] = "Comment deleted successfully!";
            }
            return RedirectToAction("Index");
        }


        public double CalculateAverageRating(int hotelId)
        {
            var ratings = unitOfWork.RatingRepository.Get(where: r => r.HotelId == hotelId);
            if (ratings.Any())
            {
                return ratings.Average(r => r.Value);
            }
            return 0.0;
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

                TempData["success"] = "Logo deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error occurred while deleting the logo: " + ex.Message;
            }

            return RedirectToAction("AddLogo");
        }
        private static ContactUs GetContactUs(string userId, string name, ContactUs.RequestType requestType, string RequestString, int commentId)
        {
            var contactUs = new ContactUs
            {
                UserId = userId,
                Name = name,
                Request = requestType,
                UserRequestString = $"Comment Report: \r\n{RequestString}",
                HelperId = commentId,
            };
            return contactUs;
        }





    }
}

