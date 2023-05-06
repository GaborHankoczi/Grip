using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

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
