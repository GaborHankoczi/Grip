namespace Grip.Bll.DTO;

public record ClassDTO(int Id, string Name, DateTime StartDateTime, UserInfoDTO Teacher, GroupDTO Group);