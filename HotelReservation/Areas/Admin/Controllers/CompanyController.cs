using AutoMapper;
using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Models.ViewModels;
using Utilities.Utility;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.AdminRole)]
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly ICompanyRepository companyRepository;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public CompanyController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ICompanyRepository companyRepository, IMapper mapper)
        {
            this.companyRepository = companyRepository;
            this.mapper = mapper;
            this.userManager = userManager;

            this.roleManager = roleManager;
        }
        public IActionResult Index()
        {
            var Company = companyRepository.Get();
            return View(Company);

        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CompanyViewModel companyVM,IFormFile ProfileImage)
        {
            ModelState.Remove(nameof(ProfileImage));
            if (ModelState.IsValid)
            {

                var newUser = mapper.Map<ApplicationUser>(companyVM);
                var email = companyVM.Email;
                newUser.UserName = email;
                var result = await userManager.CreateAsync(newUser, companyVM.Passwords);

                if (result.Succeeded)
                {
                    companyRepository.CreateProfileImage(newUser, ProfileImage);
                    if (!await roleManager.RoleExistsAsync(SD.CompanyRole))
                    {
                        await roleManager.CreateAsync(new IdentityRole(SD.CompanyRole));
                    }

                    await userManager.AddToRoleAsync(newUser, SD.CompanyRole);

                    var company = mapper.Map<Models.Models.Company>(companyVM);
                    company.UserName = newUser.UserName;
                    company.ProfileImage = newUser.ProfileImage;
                    company.Passwords = newUser.PasswordHash;
                    companyRepository.Create(company);
                    companyRepository.Commit();

                    return RedirectToAction(nameof(Index));
                }
                else
                {

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(companyVM);
        }
        public IActionResult Edit(int id)
        {
            var company = companyRepository.GetOne(where: e => e.Id == id);
            if (company == null) return NotFound();

            return View(company);
        }
        [HttpPost]
        public IActionResult Edit(Models.Models.Company company)
        {
            if (ModelState.IsValid)
            {
                companyRepository.Update(company);
                companyRepository.Commit();

                TempData["SuccessMessage"] = "Company details updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "There was an error updating the company details.";
            return View(company);
        }
        public IActionResult DeleteConfirmed(int id)
        {
            var company = companyRepository.GetOne(where: e => e.Id == id);
            if (company == null)
                return NotFound();

            companyRepository.Delete(company);
            companyRepository.Commit();

            TempData["SuccessMessage"] = "Company deleted successfully.";
            return RedirectToAction(nameof(Index));
        }


    }
}
