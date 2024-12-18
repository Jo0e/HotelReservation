using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Utilities.Utility;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.CompanyRole)]
    public class CouponController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<CouponController> logger;
        private readonly UserManager<IdentityUser> userManager;

        public CouponController(IUnitOfWork unitOfWork,ILogger<CouponController> logger,UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.userManager = userManager;
        }
        public IActionResult Index(string? search, int pageNumber = 1)
        {
            const int pageSize = 8;
            var coupons = unitOfWork.CouponRepository.Get();
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                coupons = coupons.Where(c =>
                    c.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Discount.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.ExpireDate.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));
            }
            var totalItems = coupons.Count();
            var PagedCoupons = coupons
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.SearchText = search;
            return View(PagedCoupons);
        }
        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.CouponRepository.Create(coupon);
                await unitOfWork.CompleteAsync();
                TempData["success"] = "Coupon created successfully.";
                Log(nameof(Create), nameof(coupon) + " " + $"{coupon.Code}");
                return RedirectToAction(nameof(Index));
            }

            return View(model: coupon);
        }
        public IActionResult Edit(int couponId)
        {
            var coupon = unitOfWork.CouponRepository.GetOne(where: o => o.Id == couponId);

            if (coupon != null)
            {
                return View(model: coupon);
            }

            return RedirectToAction("NotFound", "Home", new { area = "Customer" });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.CouponRepository.Update(coupon);
                await unitOfWork.CompleteAsync();
                TempData["success"] = "Coupon updated successfully.";
                Log(nameof(Edit), nameof(coupon) + " " + $"{coupon.Code}");
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        public async Task<IActionResult> DeleteAsync(int couponId)
        {
            var coupon = unitOfWork.CouponRepository.GetOne(where: e => e.Id == couponId);

            if (coupon == null)
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });

            unitOfWork.CouponRepository.Delete(coupon);

            await unitOfWork.CompleteAsync();
            TempData["success"] = "Company deleted successfully.";
            Log(nameof(DeleteAsync), nameof(coupon) + " " + $"{coupon.Code}");
            return RedirectToAction(nameof(Index));
        }
        public async void Log(string action, string entity)
        {
            LoggerHelper.LogAdminAction(logger, User.Identity.Name, action, entity);

        }
    }
}

