using Grip.Bll.DTO;
using Grip.DAL.Model;

namespace Grip.Bll.Services.Interfaces;
/// <summary>
/// Represents an interface for the exempt service.
/// </summary>
public interface IExemptService : IRestInterface<CreateExemptDTO, ExemptDTO, ExemptDTO>
{
    /// <summary>
    /// Creates a new exempt with the specified data and assigns it to a teacher.
    /// </summary>
    /// <param name="dto">The data for creating the exempt.</param>
    /// <param name="teacher">The teacher who issued the exempt.</param>
    /// <returns>A task representing the asynchronous creation of the exempt.</returns>
    public Task<ExemptDTO> Create(CreateExemptDTO dto, User teacher);
}