using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

/// <summary>
/// Represents the data transfer object for registering a user.
/// </summary>
/// <param name="Email">Eamil address of the user</param>
/// <param name="Name">name of the user</param>
/// <returns></returns>
public record RegisterUserDTO(
    [Required]
    [EmailAddress]
     string Email,
    [Required]
    [RegularExpression(Grip.Utils.Consts.UserNameRegex,ErrorMessage = "Invalid name")]
     string Name
);
