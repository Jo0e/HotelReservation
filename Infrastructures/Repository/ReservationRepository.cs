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
        public Reservation GetOneReservation(int reservationId)
        {
            var reservation = dbSet.Include(e=>e.ReservationRooms)
                .ThenInclude(r=>r.Room)
                .Include(h=>h.Hotel)
                .FirstOrDefault(e=>e.Id==reservationId);
            return reservation;
        }
    }
}
