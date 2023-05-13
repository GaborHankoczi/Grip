using AutoMapper;
using Grip.Bll.DTO;
using Grip.DAL.Model;

namespace Grip.DAL
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
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
        }
    }
}