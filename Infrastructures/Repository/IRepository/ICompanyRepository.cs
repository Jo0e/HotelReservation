using Microsoft.AspNetCore.Http;
using Models.Models;

namespace Infrastructures.Repository.IRepository
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void CreateProfileImage(ApplicationUser entity, IFormFile imageFile);
        void UpdateProfileImage(ApplicationUser entity, IFormFile newImageFile);
        void DeleteProfileImage(Company company);
    }
}
