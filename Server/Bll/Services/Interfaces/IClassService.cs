using Grip.Bll.DTO;
using Grip.DAL.Model;

namespace Grip.Bll.Services.Interfaces;
public interface IClassService : IRestInterface<CreateClassDTO,ClassDTO, ClassDTO>
{
    public Task<IEnumerable<ClassDTO>> GetClassesForUserOnDay(User user, DateOnly date);
}