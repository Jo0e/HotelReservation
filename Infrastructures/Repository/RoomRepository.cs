using Infrastructures.Data;
using Infrastructures.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using Models.ViewModels;

namespace Infrastructures.Repository
{
    public class RoomRepository : Repository<Room>, IRoomRepository
    {
        public RoomRepository(ApplicationDbContext context) : base(context)
        {
        }


        public ICollection<Room> AvailableRooms(ReservationViewModel viewModel, TypeViewModel typeModel)
        {
            var rooms = dbSet.Include(e => e.RoomType)
                .Where(
                 r => r.HotelId == typeModel.HotelId
                         && r.RoomType.Type == typeModel.RoomType
                         && r.RoomType.PricePN == typeModel.PricePN
                         && r.RoomType.MaxPersons >= viewModel.NAdult
                         && (!viewModel.IncludesMeal || r.RoomType.MealPrice == typeModel.MealPrice)
                         && (r.ReservationRooms == null || !r.ReservationRooms.Any() ||
                             r.ReservationRooms.All(rr =>
                                 rr.Reservation.CheckOutDate < viewModel.CheckInDate ||
                                 rr.Reservation.CheckInDate > viewModel.CheckOutDate
                             )
                ));
            return rooms.ToList();
        }


        public ICollection<Room> NearCheckout(ReservationViewModel viewModel, TypeViewModel typeModel)
        {
            var currentDate = DateTime.Now.Date;
            var sixDaysAfterToday = currentDate.AddDays(6);

            var rooms = dbSet.Include(r => r.RoomType)
                   .Include(r => r.ReservationRooms)
                   .ThenInclude(rr => rr.Reservation)
                   .Where(r => r.HotelId == typeModel.HotelId
                               && r.RoomType.Type == typeModel.RoomType
                               && r.ReservationRooms.Any(rr => rr.Reservation.CheckOutDate > currentDate && rr.Reservation.CheckOutDate <= sixDaysAfterToday))
                   .ToList();

            return rooms;
        }


        public ICollection<Room> NextAvailable(TypeViewModel typeModel)
        {
            var rooms = dbSet.Include(e => e.ReservationRooms)
                .Where(
                     r => r.HotelId == typeModel.HotelId
                             && r.RoomType.Type == typeModel.RoomType
                             && r.RoomType.PricePN == typeModel.PricePN
                             && !r.IsAvailable
                );
            return rooms.ToList();
        }


    }
}
