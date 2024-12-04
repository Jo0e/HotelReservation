using Infrastructures.Repository.IRepository;
using System;

namespace Infrastructures.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IHotelRepository HotelRepository { get; }
        ICompanyRepository CompanyRepository { get; }
        ICouponRepository CouponRepository { get; }
        IHotelAmenitiesRepository HotelAmenitiesRepository { get; }
        IImageListRepository ImageListRepository { get; }
        IReportRepository ReportRepository { get; }
        IReservationRepository ReservationRepository { get; }
        IRoomRepository RoomRepository { get; }
        IRoomTypeRepository RoomTypeRepository { get; }
        IUserRepository UserRepository { get; }
        IAmenityRepository AmenityRepository { get; }
        IReservationRoomRepository ReservationRoomRepository { get; }
        int Complete();
    }
}
