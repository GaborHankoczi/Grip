using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

public record CreateExemptDTO([Required] int IssuedToId, [Required] DateTime ValidFrom, [Required] DateTime ValidTo);