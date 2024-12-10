using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;

namespace HotelReservation.Areas.Customer.Controllers
{
    [Area("Customer")]
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
            var messages = unitOfWork.MessageRepository.Get(where: m => m.UserId == user.Id);
            return View(messages);
        }
        public IActionResult Delete(int id)
        {
            var toDelete = unitOfWork.MessageRepository.GetOne(where: a => a.Id == id);
            if (toDelete != null)
            {
                unitOfWork.MessageRepository.Delete(toDelete);
                unitOfWork.Complete();
            }
            return RedirectToAction("Index");
        }
    }
}
