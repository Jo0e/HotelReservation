using Infrastructures.Data;
using Infrastructures.Repository.IRepository;
using Models.Models;

namespace Infrastructures.Repository
{
    public class HotelAmenitiesRepository : Repository<HotelAmenities>, IHotelAmenitiesRepository
    {
        public HotelAmenitiesRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
