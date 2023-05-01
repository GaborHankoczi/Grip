using System.ComponentModel.DataAnnotations;

namespace Grip.DAL.DTO.Exempt;

public record ExemptCreateRequestDTO([Required] int IssuedToId, [Required] DateTime ValidFrom, [Required] DateTime ValidTo);