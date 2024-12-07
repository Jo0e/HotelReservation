using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using Models.ViewModels;
using Stripe;
using Utilities.Utility;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUnitOfWork unitOfWork;

        public UserController(UserManager<IdentityUser> userManager, IUnitOfWork unitOfWork)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
        }

        // GET: UserController
        public async Task<ActionResult> Index()
        {
            var users = userManager.Users.ToList();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    User = (ApplicationUser)user,
                    Roles = roles
                });
            }

            return View(userViewModels);
        }

        // GET: UserController/Details/5
        public async Task<ActionResult> Lockout(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (await userManager.GetLockoutEndDateAsync(user) > DateTime.Now)
                {
                    // Unlock user
                    var unlockResult = await userManager.SetLockoutEndDateAsync(user, DateTime.Now.AddMinutes(-1));
                    if (!unlockResult.Succeeded)
                    {
                        foreach (var error in unlockResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    // Lock user
                    var lockResult = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                    if (!lockResult.Succeeded)
                    {
                        foreach (var error in lockResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("NotFound", "Home", new { area = "Customer" });
        }


        // GET: UserController/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            var appUser = user as ApplicationUser;
            var role = await userManager.GetRolesAsync(user);
            ViewBag.Role = role;

            return View(appUser);
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ApplicationUser user, IFormFile ProfileImage)
        {
            try
            {

                var existingUser = await userManager.FindByIdAsync(user.Id);
                var appUser = existingUser as ApplicationUser;
                if (appUser == null)
                {
                    return NotFound($"User with ID {user.Id} not found.");
                }

                // Update user's profile image
                unitOfWork.UserRepository.UpdateProfileImage(appUser, ProfileImage);

                // Update other user properties
                appUser.Email = user.Email;
                appUser.UserName = user.Email;
                appUser.PhoneNumber = user.PhoneNumber;
                appUser.City = user.City; 
                var result = await userManager.UpdateAsync(appUser);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty,error.Description);
                }
                return View(user); 
            }
            catch
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
        }

        // GET: UserController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
