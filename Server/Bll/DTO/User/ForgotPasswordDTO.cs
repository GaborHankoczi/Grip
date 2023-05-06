
using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

public record ForgotPasswordDTO
(
    [Required]
    [EmailAddress] string Email);