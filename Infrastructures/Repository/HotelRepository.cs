using Models.Models;
using Infrastructures.Data;
using Infrastructures.Repository.IRepository;

namespace Infrastructures.Repository
{
    public class HotelRepository : Repository<Hotel>, IHotelRepository
    {
        public HotelRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
