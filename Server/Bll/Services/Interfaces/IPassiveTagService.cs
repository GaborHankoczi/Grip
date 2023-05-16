using Grip.Bll.DTO;

namespace Grip.Bll.Services.Interfaces;

/// <summary>
/// Represents an interface for the passive tag service.
/// </summary>
public interface IPassiveTagService : IRestInterface<CreatePassiveTagDTO, UpdatePassiveTagDTO, PassiveTagDTO>
{

}