using System.ComponentModel.DataAnnotations;

namespace Grip.DAL.DTO.PassiveTag;

public record PassiveTagCreationRequestDTO([Required] Int64 SerialNumber, [Required] int UserId);
