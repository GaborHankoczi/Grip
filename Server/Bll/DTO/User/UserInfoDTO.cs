namespace Grip.Bll.DTO;

/// <summary>
/// Descriptive information about a user
/// </summary>
/// <param name="Id">Id of the user</param>
/// <param name="UserName">Username of the user</param>
/// <returns></returns>
public record UserInfoDTO(int Id, string UserName);
