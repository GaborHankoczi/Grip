using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

/// <summary>
/// DTO for creating a passive tag
/// </summary>
/// <param name="SerialNumber">Serial number of the passive tag</param>
/// <param name="UserId">Id of the owner of the tag</param>
public record CreatePassiveTagDTO([Required] Int64 SerialNumber, [Required] int UserId);
