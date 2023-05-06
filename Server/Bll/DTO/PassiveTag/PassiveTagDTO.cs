using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

public record PassiveTagDTO(int Id,  Int64 SerialNumber,  UserInfoDTO User);