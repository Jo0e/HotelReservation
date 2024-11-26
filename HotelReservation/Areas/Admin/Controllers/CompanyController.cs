using AutoMapper;
using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Models.ViewModels;
using Utilities.Utility;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
        public async Task<IActionResult> Edit(CompanyViewModel companyVM , IFormFile ProfileImage)
        {
            ModelState.Remove(nameof(ProfileImage));
            ModelState.Remove(nameof(companyVM.Passwords));
            ModelState.Remove(nameof(companyVM.ConfirmPassword));
            
            if (ModelState.IsValid)
            {
                var company = companyRepository.GetOne(where:e => e.Id == companyVM.Id ,tracked:false);
                var user= await userManager.FindByEmailAsync(company.Email);
                var appUser = user as ApplicationUser;

                appUser.Email=company.Email;
                appUser.PhoneNumber=company.PhoneNumber;
                appUser.City=company.Addres;
                companyRepository.UpdateProfileImage(appUser,ProfileImage);
                
                var result = await userManager.UpdateAsync(appUser);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(companyVM);
                }
                mapper.Map(companyVM,company);
                company.ProfileImage = appUser.ProfileImage;
                companyRepository.Update(company);
                companyRepository.Commit();

                return RedirectToAction(nameof(Index));
            }

            return View(companyVM);
        }
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = companyRepository.GetOne(where: e => e.Id == id);
            if (company == null)
                return NotFound();

            var user = await userManager.FindByEmailAsync(company.Email);
            userManager.DeleteAsync(user);
            Thread.Sleep(500);
            companyRepository.DeleteProfileImage(company);
            companyRepository.Delete(company);
            companyRepository.Commit();

            return RedirectToAction(nameof(Index));
        }


    }
}
