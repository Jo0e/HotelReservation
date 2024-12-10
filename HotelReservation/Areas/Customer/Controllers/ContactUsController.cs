using Infrastructures.Repository;
using Microsoft.AspNetCore.Http;
using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.Models;
using Models.ViewModels;

using Stripe.Checkout;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HotelReservation.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ContactUsController : Controller
    {

        private readonly IUnitOfWork unitOfWork;

        public ContactUsController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ContactUs contactUs, IFormFile ImgFile)
        {
            ModelState.Remove(nameof(ImgFile));
            if (ModelState.IsValid)
            {
                unitOfWork.ContactUsRepository.CreateWithImage(contactUs, ImgFile, "ContactUsImages", "UserImgRequest");
                unitOfWork.Complete();
                return RedirectToAction(nameof(Create));
            }
            return View(model: contactUs);
        }

        [HttpGet]
        public IActionResult Edit(int contactUsId)
        {
            var contactUs = unitOfWork.ContactUsRepository.GetOne(where: c => c.Id == contactUsId);
            if (contactUs != null)
            {
                return View(model: contactUs);
            }

            return RedirectToAction("NotFound", "Home", new { area = "Customer" });
        }

        [HttpPost]
        public IActionResult Edit(ContactUs contactUs, IFormFile ImgeFile)
        {
            ModelState.Remove(nameof(ImgeFile));
            if (ModelState.IsValid)
            {
                var oldContactUs = unitOfWork.ContactUsRepository.GetOne(where: e => e.Id == contactUs.Id);
                if (oldContactUs == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });

                unitOfWork.ContactUsRepository.UpdateImage(contactUs, ImgeFile, oldContactUs.UserImgRequest, "ContactUsImages", "UserImgRequest");
                unitOfWork.Complete();

                return RedirectToAction(nameof(Create));
            }
            return View(contactUs);
        }

        [HttpGet]
        public IActionResult Delete(int contactUsId)
        {
            var contactUs = unitOfWork.ContactUsRepository.GetOne(where: e => e.Id == contactUsId);



            return View(model: contactUs);
        }
        [HttpPost]
        public IActionResult Delete(ContactUs contactUs, IFormFile ImgFile)
        {
            var oldContactUs = unitOfWork.ContactUsRepository.GetOne(where: e => e.Id == contactUs.Id);
            if (oldContactUs == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            unitOfWork.ContactUsRepository.DeleteWithImage(contactUs, "ContactUsImages", contactUs.UserImgRequest);
            unitOfWork.Complete();

            return RedirectToAction(nameof(Create));
        }
    }
}
