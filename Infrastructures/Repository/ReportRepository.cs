
using Infrastructures.Data;
using Infrastructures.Repository.IRepository;
using Models.Models;

namespace Infrastructures.Repository
{
    public class ReportRepository : Repository<Report>, IReportRepository
    {
        public ReportRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
