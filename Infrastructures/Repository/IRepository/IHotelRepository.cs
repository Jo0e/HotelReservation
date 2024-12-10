using Models.Models;

namespace Infrastructures.Repository.IRepository
{
    public interface IHotelRepository : IRepository<Hotel>
    {
        Hotel HotelsWithAmenities(int id);
        ICollection<Hotel> HotelsWithCity(string city);
    }
}
