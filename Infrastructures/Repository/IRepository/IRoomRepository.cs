
using Models.Models;
using Models.ViewModels;

namespace Infrastructures.Repository.IRepository
{
    public interface IRoomRepository : IRepository<Room>
    {
        ICollection<Room> AvailableRooms(ReservationViewModel viewModel, TypeViewModel typeModel);
        ICollection<Room> NearCheckout(ReservationViewModel viewModel, TypeViewModel typeModel);
        ICollection<Room> NextAvailable(TypeViewModel typeModel);
    }
}
