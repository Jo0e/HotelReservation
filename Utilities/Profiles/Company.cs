using AutoMapper;
using Models.Models;
using Models.ViewModels;

namespace Utilities.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CompanyViewModel, ApplicationUser>()
             .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
             .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
             .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Addres))
             .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
             .ForMember(dest => dest.ProfileImage, opt => opt.MapFrom(src => src.ProfileImage))
            .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<CompanyViewModel, Company>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Addres, opt => opt.MapFrom(src => src.Addres))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.ProfileImage, opt => opt.MapFrom(src => src.ProfileImage))
                .ForMember(dest => dest.Passwords, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());


            CreateMap<Company, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Addres))
            .ForMember(dest => dest.ProfileImage, opt => opt.MapFrom(src => src.ProfileImage))
            .ForMember(dest => dest.Id, opt => opt.Ignore());




            CreateMap<Room, Room>()
           .ForMember(dest => dest.RoomTypeId, opt => opt.MapFrom(src => src.RoomTypeId))
           .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailable))
           .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => src.HotelId))
           .ForMember(dest => dest.Id, opt => opt.Ignore())
           .ForMember(dest => dest.ReservationRooms, opt => opt.Ignore())
           .ForMember(dest => dest.RoomType, opt => opt.Ignore())
           .ForMember(dest => dest.Hotel, opt => opt.Ignore());

        }

    }
}
