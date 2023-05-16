using Grip.Bll.DTO;
using Grip.DAL.Model;

namespace Grip.Bll.Services.Interfaces;
public interface IClassService : IRestInterface<CreateClassDTO, ClassDTO, ClassDTO>
{
    /// <summary>
    /// Retrieves classes for a user on a specific day.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="date">The date.</param>
    /// <returns>A list of class DTOs.</returns>
    public Task<IEnumerable<ClassDTO>> GetClassesForUserOnDay(User user, DateOnly date);
}