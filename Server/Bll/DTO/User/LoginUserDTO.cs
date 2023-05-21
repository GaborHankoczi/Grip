using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

/// <summary>
/// DTO for the login request
/// </summary>
/// <param name="Email">Email of the user</param>
/// <param name="Password">Password of the user</param>
/// <returns></returns>
public record LoginUserDTO([Required][EmailAddress] string Email, [Required] string Password);

