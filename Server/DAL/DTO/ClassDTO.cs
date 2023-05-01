namespace Grip.DAL.DTO;

public record ClassDTO(int Id, string Name, DateTime StartDateTime, UserInfoDTO Teacher, GroupDTO Group);