
using Models.Models;

namespace Infrastructures.Repository.IRepository
{
    public interface IReservationRepository : IRepository<Reservation>
    {
        IEnumerable<Reservation> GetReservationRoom(string userId);
        Reservation GetOneReservation(int reservationId);
    }
}
