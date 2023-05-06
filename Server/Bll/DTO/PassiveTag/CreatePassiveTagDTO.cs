using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

public record CreatePassiveTagDTO([Required] Int64 SerialNumber, [Required] int UserId);
