using System.ComponentModel.DataAnnotations;

namespace Grip.DAL.DTO;

public record GroupDTO([Required]int Id,[Required] string Name);