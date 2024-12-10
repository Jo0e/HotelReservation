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
            var hotel = dbSet.Include(e => e.HotelAmenities)
            .ThenInclude(a => a.Amenity).FirstOrDefault(e => e.Id == id);
            if (hotel == null)
            {
                return null;
            }
            return hotel;
        }

        public ICollection<Hotel> HotelsWithCity(string city)
        {
            if (city != null)
            {
                var hotels = dbSet.Where(e => e.City == city)
                    .Include(r => r.Rooms)
                    .Include(a => a.HotelAmenities)
                    .ThenInclude(a => a.Amenity).ToList();
                return hotels;
            }
            else
            {
                var hotels = dbSet
                    .Include(r => r.Rooms)
                    .Include(a => a.HotelAmenities)
                    .ThenInclude(a => a.Amenity).ToList();
                return hotels;
            }
        }
    }
}
