using Infrastructures.Data;
using Infrastructures.Repository.IRepository;
using Models.Models;

namespace Infrastructures.Repository
{
    public class RoomRepository : Repository<Room>, IRoomRepository
    {
        public RoomRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
