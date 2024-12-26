using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Utilities.Utility;

namespace HotelReservation.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class InboxController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<IdentityUser> userManager;

        public InboxController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            var messages = unitOfWork.MessageRepository.Get(where: m => m.UserId == user.Id)
                .OrderByDescending(d=>d.MessageDateTime);
            return View(messages);
        }
        public IActionResult ReadMessage(int messageId)
        {
            var message = unitOfWork.MessageRepository.GetOne(where: m => m.Id == messageId);
            if (message == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            if (message.IsReadied == false)
            {
                message.IsReadied = true;
                unitOfWork.MessageRepository.Update(message);
                unitOfWork.Complete();
            }
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var toDelete = unitOfWork.MessageRepository.GetOne(where: a => a.Id == id);
            if (toDelete != null)
            {
                unitOfWork.MessageRepository.Delete(toDelete);
                unitOfWork.Complete();
                TempData["success"] = "Message deleted successfully.";
            }
            return RedirectToAction("Index");
        }
    }
}
