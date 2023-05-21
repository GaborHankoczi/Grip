
using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

/// <summary>
/// DTO for the forgot password request
/// </summary>
/// <param name="Email">Email to send the recovery link to</param>
public record ForgotPasswordDTO
(
    [Required]
    [EmailAddress] string Email);