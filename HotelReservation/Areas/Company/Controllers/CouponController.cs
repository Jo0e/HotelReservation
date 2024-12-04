using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models.Models;

namespace HotelReservation.Areas.Company.Controllers
{
    public class CouponController : Controller
    {
        private readonly ICouponRepository couponRepository;
        public CouponController(ICouponRepository couponRepository)
        {
            this.couponRepository = couponRepository;
        }
        public IActionResult Index()
        {
            var coupons = couponRepository.Get();

            return View(model: coupons);
        }
        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }
        [HttpPost]
        public IActionResult Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                couponRepository.Create(coupon);
                couponRepository.Commit();
                return RedirectToAction(nameof(Index));
            }

            return View(model: coupon);
        }
        public IActionResult Edit(int couponId)
        {
            var coupon = couponRepository.GetOne(where: o => o.Id == couponId);

            if (coupon != null)
            {
                return View(model: coupon);
            }

            return RedirectToAction("NotFound", "Home");

        }

        [HttpPost]
        public IActionResult Edit(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                couponRepository.Update(coupon);
                couponRepository.Commit();

                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        public IActionResult Delete(int couponid)
        {
            var coupon = couponRepository.GetOne(where: e => e.Id == couponid);

            if (coupon == null)
                RedirectToAction("NotFound", "Home");

            couponRepository.Delete(coupon);
            couponRepository.Commit();

            return RedirectToAction(nameof(Index));
        }
    }
}

