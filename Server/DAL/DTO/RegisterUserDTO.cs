using System;
using System.ComponentModel.DataAnnotations;

namespace Grip.DAL.DTO;

public record RegisterUserDTO(
    [Required] 
    [EmailAddress]    
     string Email,
    [Required]
    [RegularExpression(Grip.Utils.Consts.UserNameRegex,ErrorMessage = "Invalid name")]
     string Name
);
