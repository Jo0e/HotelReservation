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
            .ForMember(dest => dest.CouponId, opt => opt.Ignore())       
            .ForMember(dest => dest.Status, opt => opt.Ignore())         
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.NChildren, opt => opt.MapFrom(src => src.NChildren ?? 0));


        }
    }
}
