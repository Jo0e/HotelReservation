using Infrastructures.Data;
using Infrastructures.Repository.IRepository;
using Models.Models;

namespace Infrastructures.Repository
{
    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        public ReservationRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
