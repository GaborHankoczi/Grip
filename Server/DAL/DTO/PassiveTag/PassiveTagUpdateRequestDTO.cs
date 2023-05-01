using System.ComponentModel.DataAnnotations;

namespace Grip.DAL.DTO.PassiveTag;

public record PassiveTagUpdateRequestDTO([Required] int Id, [Required] Int64 SerialNumber, [Required] int UserId);
