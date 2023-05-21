using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

/// <summary>
/// DTO for a passive tag
/// </summary>
/// <param name="Id">Id of the tag</param>
/// <param name="SerialNumber">Serial number of the tag</param>
/// <param name="User">Information of the owner</param>
/// <returns></returns>
public record PassiveTagDTO(int Id, Int64 SerialNumber, UserInfoDTO User);