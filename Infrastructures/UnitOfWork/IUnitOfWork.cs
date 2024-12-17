using Infrastructures.Repository.IRepository;
using Microsoft.EntityFrameworkCore.Storage;
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
        IRatingRepository RatingRepository { get; }
        ICommentRepository CommentRepository { get; }
        IContactUsRepository ContactUsRepository { get; }
        IMessageRepository MessageRepository { get; }
        int Complete();
        Task CompleteAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
