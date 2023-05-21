using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

/// <summary>
/// DTO for updating a passive tag
/// </summary>
/// <param name="Id">Id of the tag</param>
/// <param name="SerialNumber">Serial number of the tag</param>
/// <param name="UserId">Id of the owner</param>
public record UpdatePassiveTagDTO([Required] int Id, [Required] Int64 SerialNumber, [Required] int UserId);