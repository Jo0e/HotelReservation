﻿using HotelReservation.Hubs;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.IdentityModel.Tokens;
using Models.Models;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using Utilities.Utility;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class ContactUsController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<ContactUs> logger;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IHubContext<NotificationHub> hubContext;

        public ContactUsController(IUnitOfWork unitOfWork, ILogger<ContactUs> logger,
            UserManager<IdentityUser> userManager,IHubContext<NotificationHub> hubContext)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.userManager = userManager;
            this.hubContext = hubContext;
        }

        public IActionResult Index()
        {
            var contactMessage = unitOfWork.ContactUsRepository.Get().OrderByDescending(a=>a.Id);
            return View(contactMessage);
        }
        public IActionResult Details(int reqId)
        {
            var req = unitOfWork.ContactUsRepository.GetOne(where: c => c.Id == reqId);
            if (req == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            if (req.HelperId != null && req.HelperId != 0)
            {
                ViewBag.Comment = unitOfWork.CommentRepository.GetOne(where: e => e.Id == req.HelperId
                , include: [s => s.User]);
            }
            if (req.IsReadied == false)
            {
                ReadMessage(req);
            }
            return View(req);
        }
        public async Task<IActionResult> Respond(Models.Models.Message message)
        {
            message.MessageDateTime = DateTime.Now;
            if (ModelState.IsValid)
            {
                unitOfWork.MessageRepository.Create(message);
                unitOfWork.Complete();

                // Notify clients
                var messageInfo = JsonConvert.SerializeObject(new
                {
                    message.Id,
                    message.Title,
                    message.MessageString,
                    message.Description,
                    message.MessageDateTime,
                    message.IsReadied,
                    message.UserId
                });
                var userMessageCount = unitOfWork.MessageRepository.Get(where: m => m.UserId == message.UserId &
                m.IsReadied == false,tracked:false).Count();

                await hubContext.Clients.Group("Customers").SendAsync("CustomerNotification", messageInfo,userMessageCount, message.UserId);

                Log(nameof(Respond), nameof(message) + " " + $"{message.Title}");
                TempData["success"] = "Response sent successfully.";
                return RedirectToAction("Index");
            }

            return RedirectToAction("NotFound", "Home", new { area = "Customer" });
        }

        public IActionResult ReadMessage(int messageId)
        {
            var contact = unitOfWork.ContactUsRepository.GetOne(where: m => m.Id == messageId);
            if (contact == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            if (contact.IsReadied == false)
            {
                ReadMessage(contact);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> ApproveCompanyRequest(string userId)
        {
            var user = await userManager.FindByIdAsync(userId) as ApplicationUser;
            if (user == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            var oldRole = await userManager.GetRolesAsync(user);
            if (oldRole.Contains(SD.CompanyRole))
            {
                return Json(new { success = false, message = "The user is already assigned to the company role." });
            }
            await userManager.RemoveFromRolesAsync(user, oldRole);
            var result = await userManager.AddToRoleAsync(user, SD.CompanyRole);
            if (result.Succeeded)
            {
                var company = new Models.Models.Company
                {
                    Email = user.Email,
                    Addres = user.City,
                    Passwords = user.PasswordHash,
                    PhoneNumber = user.PhoneNumber,
                    ProfileImage = user.ProfileImage,
                    UserName = user.UserName,
                };

                unitOfWork.CompanyRepository.Create(company);

                var message = new Models.Models.Message
                {
                    UserId = user.Id,
                    Title = "Your Company Request",
                    MessageString=$"Welcome {user.Email} we have Approved your Company Request \r\n" +
                    $"now your account is in role to mange Hotels as Company",
                    Description = "We looking for the business with you",
                    MessageDateTime = DateTime.Now,
                };
                unitOfWork.MessageRepository.Create(message);

                await unitOfWork.CompleteAsync();


                Log(nameof(ApproveCompanyRequest), nameof(User) + " " + $"{user.Email}");
                return Json(new { success = true, message = "User assigned to company role successfully." });
            }
            return RedirectToAction("NotFound", "Home", new { area = "Customer" });
        }

        public async Task<IActionResult> Delete(int reqId)
        {
            var toDelete = unitOfWork.ContactUsRepository.GetOne(where: o => o.Id == reqId);
            if (toDelete != null)
            {
                unitOfWork.ContactUsRepository.DeleteWithImage(toDelete, "ContactUsImages", toDelete.UserImgRequest);
                await unitOfWork.CompleteAsync();
                Log(nameof(Delete), "Request" + " " + $"{toDelete.UserRequestString}");
                TempData["success"] = "Message deleted successfully.";

            }
            return RedirectToAction("Index");
        }

        private void ReadMessage(ContactUs req)
        {
            req.IsReadied = true;
            unitOfWork.ContactUsRepository.Update(req);
            unitOfWork.Complete();
        }
        public async void Log(string action, string entity)
        {
            LoggerHelper.LogAdminAction(logger, User.Identity.Name, action, entity);

        }

    }
}
