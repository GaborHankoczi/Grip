using System.ComponentModel.DataAnnotations;

namespace Grip.DAL.DTO;

public record ClassCreationRequestDTO([Required] string Name, [Required] DateTime StartDateTime, [Required] int GroupId, [Required] int TeacherId);