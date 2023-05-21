using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

/// <summary>
/// DTO for confirming the email address of a user
/// </summary>
/// <param name="Email">Email of the user</param>
/// <param name="Token">Token sent to the email previously</param>
/// <param name="Password">Password to be set for the user</param>
public record ConfirmEmailDTO([Required][EmailAddress] string Email, [Required] string Token, [Required][RegularExpression(Grip.Utils.Consts.UserPasswordRegex)] string Password);
