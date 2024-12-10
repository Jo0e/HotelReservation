using Infrastructures.Data;
using Infrastructures.Repository.IRepository;
using Infrastructures.Repository;
using System;

namespace Infrastructures.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            HotelRepository = new HotelRepository(_context);
            CompanyRepository = new CompanyRepository(_context);
            CouponRepository = new CouponRepository(_context);
            HotelAmenitiesRepository = new HotelAmenitiesRepository(_context);
            ImageListRepository = new ImageListRepository(_context);
            ReportRepository = new ReportRepository(_context);
            ReservationRepository = new ReservationRepository(_context);
            RoomRepository = new RoomRepository(_context);
            RoomTypeRepository = new RoomTypeRepository(_context);
            UserRepository = new UserRepository(_context);
            ReservationRoomRepository = new ReservationRoomRepository(_context);
            AmenityRepository = new AmenityRepository(_context);
            RatingRepository = new RatingRepository(_context);
            CommentRepository = new CommentRepository(_context);
            ContactUsRepository = new ContactUsRepository(_context);

        }

        public IHotelRepository HotelRepository { get; private set; }
        public ICompanyRepository CompanyRepository { get; private set; }
        public ICouponRepository CouponRepository { get; private set; }
        public IHotelAmenitiesRepository HotelAmenitiesRepository { get; private set; }
        public IImageListRepository ImageListRepository { get; private set; }
        public IReportRepository ReportRepository { get; private set; }
        public IReservationRepository ReservationRepository { get; private set; }
        public IRoomRepository RoomRepository { get; private set; }
        public IRoomTypeRepository RoomTypeRepository { get; private set; }
        public IUserRepository UserRepository { get; private set; }
        public IReservationRoomRepository ReservationRoomRepository { get; private set; }
        public IAmenityRepository AmenityRepository { get; private set; }
        public ICommentRepository CommentRepository { get; private set; }
        public IRatingRepository RatingRepository { get; private set; }
        public IContactUsRepository ContactUsRepository { get; private set; }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
