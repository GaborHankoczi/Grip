
using System.ComponentModel.DataAnnotations;

namespace Grip.DAL.DTO;

public record ForgotPasswordDTO
(
    [Required]
    [EmailAddress] string Email);