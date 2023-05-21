using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

/// <summary>
/// DTO for the reset password request
/// </summary>
/// <param name="Email">Email of the user</param>
/// <param name="Token">Token sent to the user</param>
/// <param name="Password">New password to be set</param>
/// <returns></returns>
public record ResetPasswordDTO(
    [Required]
    [EmailAddress]
    string Email,
    [Required]
    string Token,
    [Required]
    [RegularExpression(Grip.Utils.Consts.UserPasswordRegex)]
    string Password
);
