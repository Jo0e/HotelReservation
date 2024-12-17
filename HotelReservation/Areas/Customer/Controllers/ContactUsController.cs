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
using HotelReservation.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace HotelReservation.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ContactUsController : Controller
    {

        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IHubContext<NotificationHub> hubContext;

        public ContactUsController(IUnitOfWork unitOfWork,UserManager<IdentityUser> userManager,IHubContext<NotificationHub> hubContext )
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.hubContext = hubContext;
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

                // Create a detailed object for the notification
                var notification = new
                {
                    contactUs.Id,
                    contactUs.Name,
                    RequestType = contactUs.Request.ToString(),
                    contactUs.UserRequestString
                };
                // Convert the object to JSON string
                var notificationJson = JsonSerializer.Serialize(notification);
                // Notify admin with detailed contact request info

                await hubContext.Clients.Group("Admins").SendAsync("AdminNotification", notificationJson);
                TempData["Success"] = "Your message has been submitted successfully.";
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
