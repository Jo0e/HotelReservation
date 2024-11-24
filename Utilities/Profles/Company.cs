using AutoMapper;
using Models.Models;

namespace Utilities.Profles
{
    public class Company : Profile
    {
        public Company()
        {
            CreateMap<Company, ApplicationUser>();
        }

    }
}
