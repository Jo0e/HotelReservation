using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Models.Models;

namespace HotelReservation.Areas.Company.Controllers
{
    [Area("Company")]
    public class CouponController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CouponController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var coupons = unitOfWork.CouponRepository.Get();

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
                unitOfWork.CouponRepository.Create(coupon);
                unitOfWork.Complete();
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

            return RedirectToAction("NotFound", "Home");

        }

        [HttpPost]
        public IActionResult Edit(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.CouponRepository.Update(coupon);
                unitOfWork.Complete();

                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        public IActionResult Delete(int couponid)
        {
            var coupon = unitOfWork.CouponRepository.GetOne(where: e => e.Id == couponid);

            if (coupon == null)
                RedirectToAction("NotFound", "Home");

            unitOfWork.CouponRepository.Delete(coupon);
            unitOfWork.Complete();

            return RedirectToAction(nameof(Index));
        }
    }
}

