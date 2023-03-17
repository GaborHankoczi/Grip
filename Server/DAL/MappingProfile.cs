using AutoMapper;

namespace Grip.DAL
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Model.User, DAL.DTO.UserDTO>();
            CreateMap<Model.User, DAL.DTO.LoginResultDTO>();
        }
    }
}