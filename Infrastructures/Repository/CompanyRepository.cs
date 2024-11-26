using Infrastructures.Data;
using Infrastructures.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Models.Models;

namespace Infrastructures.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void CreateProfileImage(ApplicationUser entity, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var profileFolderPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\images\\profile\\{entity.Email}");
                if (!Directory.Exists(profileFolderPath))
                {
                    Directory.CreateDirectory(profileFolderPath);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                var filePath = Path.Combine(profileFolderPath, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    imageFile.CopyTo(stream);
                }

                entity.ProfileImage = fileName;

            }

        }


    }
}
