using AutoMapper;
using Models.Models;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Profiles
{
    internal class BookingProfile:Profile
    {
        public BookingProfile() {
            CreateMap<ReservationViewModel, Reservation>()
            .ForMember(dest => dest.TotalPrice, opt => opt.Ignore()) 
            .ForMember(dest => dest.ReservationRooms, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CouponId, opt => opt.Ignore())        // Handled explicitly
            .ForMember(dest => dest.Status, opt => opt.Ignore())          // Default "Pending"
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            //CreateMap<TypeViewModel, RoomType>();
        }
    }
}
