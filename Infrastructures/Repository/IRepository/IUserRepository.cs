using Microsoft.AspNetCore.Http;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructures.Repository.IRepository
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        void CreateProfileImage(ApplicationUser entity, IFormFile imageFile);
        void UpdateProfileImage(ApplicationUser entity, IFormFile newImageFile);

    }



}
