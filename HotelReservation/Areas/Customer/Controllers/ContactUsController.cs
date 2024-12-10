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
        private readonly UserManager<IdentityUser> userManager;

        public ContactUsController(IUnitOfWork unitOfWork,UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ContactUs contactUs, IFormFile ImgFile)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            contactUs.UserId = user.Id;
            contactUs.Name = user.Email;
            ModelState.Remove(nameof(ImgFile));
            if (ModelState.IsValid)
            {
                unitOfWork.ContactUsRepository.CreateWithImage(contactUs, ImgFile, "ContactUsImages", "UserImgRequest");
                var message = ConfirmationMessage(user.Id);
                unitOfWork.MessageRepository.Create(message);
                unitOfWork.Complete();
                return RedirectToAction("Index","Home");
            }
            return View(model: contactUs);
        }


        private static Message ConfirmationMessage(string userId) 
        {
            var message = new Message
            {
                UserId = userId,
                Title = "Contact Us",
                MessageString = $"Thank you for Contacting us, \r\nYour Request Has been submitted successfully ",
                Description = "we will replay ASAP",
                MessageDateTime = DateTime.Now,
            };
            return message;
        }

    }
}
