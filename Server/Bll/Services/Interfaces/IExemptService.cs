using Grip.Bll.DTO;
using Grip.DAL.Model;

namespace Grip.Bll.Services.Interfaces;
public interface IExemptService : IRestInterface<CreateExemptDTO, ExemptDTO, ExemptDTO>
{
    public Task<ExemptDTO> Create(CreateExemptDTO dto, User teacher);
}