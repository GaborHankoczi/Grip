namespace Grip.DAL.DTO;

public record LoginResultDTO(string UserName, string Email, string[] Roles){
    public LoginResultDTO() : this("", "", new string[0]) { }
};
