using Infrastructures.Data;
using Infrastructures.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Models.Models;

namespace Infrastructures.Repository
{
    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        public ReservationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IEnumerable<Reservation> GetReservationRoom(string userId)
        {
            return dbSet.Where(s => s.UserId == userId)
                .Include(r => r.ReservationRooms)
                .ThenInclude(e => e.Room);
        }
    }
}
