namespace Grip.Bll.DTO;

public record ExemptDTO(int Id, UserInfoDTO IssuedBy, UserInfoDTO IssuedTo, DateTime ValidFrom, DateTime ValidTo);