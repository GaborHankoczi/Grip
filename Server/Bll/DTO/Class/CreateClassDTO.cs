using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

public record CreateClassDTO([Required] string Name, [Required] DateTime StartDateTime, [Required] int GroupId, [Required] int TeacherId, [Required] int StationId);