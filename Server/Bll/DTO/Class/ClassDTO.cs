namespace Grip.Bll.DTO;

/// <summary>
/// DTO for the class
/// </summary>
/// <param name="Id">Id of the class</param>
/// <param name="Name">Name of the class</param>
/// <param name="StartDateTime">Start of the class</param>
/// <param name="Teacher">Informtaion about the teacher</param>
/// <param name="Group">Infromation about the group</param>
public record ClassDTO(int Id, string Name, DateTime StartDateTime, UserInfoDTO Teacher, GroupDTO Group);