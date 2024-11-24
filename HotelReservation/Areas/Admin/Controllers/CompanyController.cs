using AutoMapper;
using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Utilities.Utility;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Authorize(SD.AdminRole)]
    public class CompanyController : Controller
    {
        private readonly ICompanyRepository companyRepository;
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public CompanyController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ICompanyRepository companyRepository ,IMapper mapper)
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
        public async Task<IActionResult> Create(Company company)
        {
            if (ModelState.IsValid)
            {

                var newUser = mapper.Map<ApplicationUser>(company);
                               
                var result = await userManager.CreateAsync(newUser, company.Passwords);

                if (result.Succeeded)
                {
                    
                    if (!await roleManager.RoleExistsAsync(SD.CompanyRole))
                    {
                        await roleManager.CreateAsync(new IdentityRole(SD.CompanyRole));
                    }

                    await userManager.AddToRoleAsync(newUser, SD.CompanyRole);

                    companyRepository.Create(company);
                    companyRepository.Commit();

                    TempData["SuccessMessage"] = "Company created and role assigned successfully.";
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

            TempData["ErrorMessage"] = "There was an error creating the company.";
            return View(company);
        }
        public IActionResult Edit(int id)
        {
            var company = companyRepository.GetOne(where:e=>e.Id==id);
            if (company == null) return NotFound();

            return View(company);
        }
        [HttpPost]
        public IActionResult Edit(Company company)
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
            var company = companyRepository.GetOne(where:e=>e.Id==id);
            if (company == null) 
                return NotFound();

            companyRepository.Delete(company);
            companyRepository.Commit();

            TempData["SuccessMessage"] = "Company deleted successfully.";
            return RedirectToAction(nameof(Index));
        }


    }
}
