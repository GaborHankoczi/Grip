using AutoMapper;

namespace Grip.DAL
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Model.User, DAL.DTO.UserDTO>().ReverseMap();
            CreateMap<Model.User, DAL.DTO.LoginResultDTO>().ReverseMap();
            CreateMap<Model.Class, DAL.DTO.ClassDTO>().ReverseMap();
            CreateMap<Model.Group, DAL.DTO.GroupDTO>().ReverseMap();
            CreateMap<Model.User, DAL.DTO.UserInfoDTO>();
            CreateMap<DAL.DTO.ClassCreationRequestDTO, Model.Class>();
            CreateMap<DAL.DTO.PassiveTag.PassiveTagDTO, Model.PassiveTag>().ReverseMap();
            CreateMap<DAL.DTO.PassiveTag.PassiveTagCreationRequestDTO, Model.PassiveTag>();
            CreateMap<DAL.DTO.PassiveTag.PassiveTagUpdateRequestDTO, Model.PassiveTag>();
            CreateMap<Model.Exempt, DAL.DTO.Exempt.ExemptDTO>().ForMember(dto=>dto.IssuedBy, opt=>opt.MapFrom(e=>e.IssuedBy)).ForMember(dto=>dto.IssuedTo, opt=>opt.MapFrom(e=>e.IssuedTo));
            CreateMap<DAL.DTO.Exempt.ExemptCreateRequestDTO, Model.Exempt>();
        }
    }
}