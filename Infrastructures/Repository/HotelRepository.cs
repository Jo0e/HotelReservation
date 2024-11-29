using Models.Models;
using Infrastructures.Data;
using Infrastructures.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructures.Repository
{
    public class HotelRepository : Repository<Hotel>, IHotelRepository
    {
        public HotelRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Hotel HotelsWithAmenities(int id)
        {
                var hotel = dbSet.Include(e=>e.HotelAmenities)
                .ThenInclude(a=>a.Amenity).FirstOrDefault(e=>e.Id == id);
            if (hotel == null)
            {
                return null;
            }
            return hotel;
        }
    }
}
