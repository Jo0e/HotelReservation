using AutoMapper;
using Infrastructures.Repository.IRepository;
using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
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
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ILogger<CompanyController> logger;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public CompanyController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork, IMapper mapper,ILogger<CompanyController> logger)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.logger = logger;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }
        [HttpGet]
        public IActionResult Index(string? search, int pageNumber = 1)
        {
            const int pageSize = 10;
            var companies = unitOfWork.CompanyRepository.Get();
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                companies = companies.Where(c =>
                    c.UserName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Addres.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var totalItems = companies.Count();
            var pagedCompanies = companies
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.SearchText = search;

            return View(pagedCompanies);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(CompanyViewModel companyVM, IFormFile ProfileImage)
        {
            ModelState.Remove(nameof(ProfileImage));
            if (ModelState.IsValid)
            {
                var newUser = Activator.CreateInstance<ApplicationUser>();
                newUser = mapper.Map<ApplicationUser>(companyVM);
                newUser.UserName = companyVM.Email;
                var result = await userManager.CreateAsync(newUser, companyVM.Passwords);

                if (result.Succeeded)
                {
                    unitOfWork.CompanyRepository.CreateProfileImage(newUser, ProfileImage);
                    if (!await roleManager.RoleExistsAsync(SD.CompanyRole))
                    {
                        await roleManager.CreateAsync(new IdentityRole(SD.CompanyRole));
                    }

                    await userManager.AddToRoleAsync(newUser, SD.CompanyRole);

                    var company = mapper.Map<Models.Models.Company>(companyVM);
                    company.UserName = newUser.UserName;
                    company.ProfileImage = newUser.ProfileImage;
                    company.Passwords = newUser.PasswordHash;
                    unitOfWork.CompanyRepository.Create(company);
                    unitOfWork.Complete();
                    TempData["success"] = "Company created successfully.";
                    Log(nameof(Create), nameof(company) + " " + $"{company.Email}");
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
            var company = unitOfWork.CompanyRepository.GetOne(where: e => e.Id == id);
            if (company == null) return NotFound();
            return View(company);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CompanyViewModel companyVM, IFormFile ProfileImage)
        {
            ModelState.Remove(nameof(ProfileImage));
            ModelState.Remove(nameof(companyVM.Passwords));
            ModelState.Remove(nameof(companyVM.ConfirmPassword));

            if (ModelState.IsValid)
            {
                var company = unitOfWork.CompanyRepository.GetOne(where: e => e.Id == companyVM.Id, tracked: false);
                if (company == null) return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                var user = await userManager.FindByEmailAsync(company.Email);
                var appUser = user as ApplicationUser;

                if (appUser == null) 
                {
                    return RedirectToAction("NotFound", "Home", new { area = "Customer" });
                }
                appUser.Email = company.Email;
                appUser.PhoneNumber = company.PhoneNumber;
                appUser.City = company.Addres;
                unitOfWork.CompanyRepository.UpdateProfileImage(appUser, ProfileImage);

                var result = await userManager.UpdateAsync(appUser);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(companyVM);
                }
                mapper.Map(companyVM, company);
                company.ProfileImage = appUser.ProfileImage;
                unitOfWork.CompanyRepository.Update(company);
                unitOfWork.Complete();
                TempData["success"] = "Company updated successfully.";
                Log(nameof(Edit), nameof(company) + " " + $"{company.Email}");
                return RedirectToAction(nameof(Index));
            }

            return View(companyVM);
        }
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = unitOfWork.CompanyRepository.GetOne(where: e => e.Id == id);
            if (company == null)
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });

            var user = await userManager.FindByEmailAsync(company.Email);
            if (user == null)
            {
                return RedirectToAction("NotFound", "Home", new { area = "Customer" });
            }
            await userManager.DeleteAsync(user);
            Thread.Sleep(500);
            unitOfWork.CompanyRepository.DeleteProfileImage(company);
            unitOfWork.CompanyRepository.Delete(company);
            Log(nameof(Delete), nameof(company) + " " + $"{company.Email}");
            unitOfWork.Complete();
            TempData["success"] = "Company deleted successfully.";

            return RedirectToAction(nameof(Index));
        }


        public async void Log(string action, string entity)
        {
            var user = await userManager.GetUserAsync(User);
            LoggerHelper.LogAdminAction(logger, user.Id, user.Email, action, entity);
        }
    }
}
