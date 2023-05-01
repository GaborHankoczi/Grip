namespace Grip.DAL.DTO.Exempt;

public record ExemptDTO(int Id, UserInfoDTO IssuedBy, UserInfoDTO IssuedTo, DateTime ValidFrom, DateTime ValidTo);