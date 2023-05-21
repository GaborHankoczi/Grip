namespace Grip.Bll.DTO;

/// <summary>
/// DTO for the login result
/// </summary>
/// <param name="UserName">Username of the user</param>
/// <param name="Email">Email address of the user</param>
/// <param name="Roles">List of roles of the user</param>
public record LoginResultDTO(string UserName, string Email, string[] Roles)
{
    /// <summary>
    /// Creates an empty instance of the <see cref="LoginResultDTO"/> class.
    /// </summary>
    public LoginResultDTO() : this("", "", new string[0]) { }
};
