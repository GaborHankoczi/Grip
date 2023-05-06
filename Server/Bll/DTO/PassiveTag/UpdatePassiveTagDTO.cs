using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

public record UpdatePassiveTagDTO([Required] int Id, [Required] Int64 SerialNumber, [Required] int UserId);