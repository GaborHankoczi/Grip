using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

public record LoginUserDTO([Required][EmailAddress]string Email, [Required]string Password);

