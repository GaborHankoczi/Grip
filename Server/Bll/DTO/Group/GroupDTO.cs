using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

public record GroupDTO([Required]int Id,[Required] string Name);