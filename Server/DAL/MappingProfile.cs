using AutoMapper;
using Grip.Bll.DTO;
using Grip.Bll.DTO.Everlink;
using Grip.Bll.Everlink;
using Grip.DAL.Model;

namespace Grip.DAL
{
    /// <summary>
    /// Mapping profile for AutoMapper.
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the MappingProfile class.
        /// </summary>
        public MappingProfile()
        {
            CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<User, LoginResultDTO>().ReverseMap();
            CreateMap<Class, ClassDTO>().ReverseMap();
            CreateMap<Group, GroupDTO>().ReverseMap();
            CreateMap<User, UserInfoDTO>();
            CreateMap<CreateClassDTO, Class>();
            CreateMap<PassiveTagDTO, PassiveTag>().ReverseMap();
            CreateMap<CreatePassiveTagDTO, PassiveTag>();
            CreateMap<UpdatePassiveTagDTO, PassiveTag>();
            CreateMap<Exempt, ExemptDTO>().ForMember(dto => dto.IssuedBy, opt => opt.MapFrom(e => e.IssuedBy)).ForMember(dto => dto.IssuedTo, opt => opt.MapFrom(e => e.IssuedTo));
            CreateMap<CreateExemptDTO, Exempt>();
            CreateMap<User, StudentDetailDTO>();
            CreateMap<Table,TableDTO>();
        }
    }
}