using Infrastructures.Data;
using Infrastructures.Repository.IRepository;
using Models.Models;

namespace Infrastructures.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
