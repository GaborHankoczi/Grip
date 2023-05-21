using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.DTO;

/// <summary>
/// DTO for the user
/// </summary>
/// <param name="Id">Id of the user</param>
/// <param name="Email">Email of the user</param>
/// <param name="UserName">Username of the user</param>
/// <param name="EmailConfirmed">Wether the user verified their email address</param>
/// <returns></returns>
public record UserDTO([Required] int Id, [Required][EmailAddress] string Email, [Required][RegularExpression(Grip.Utils.Consts.UserNameRegex)] string UserName, bool EmailConfirmed)
{
    /// <summary>
    /// Creates an empty user DTO
    /// </summary>
    public UserDTO() : this(0, "", "", false) { }
};