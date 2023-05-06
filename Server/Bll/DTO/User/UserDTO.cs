using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.DTO;

public record UserDTO([Required]int Id,[Required][EmailAddress] string Email,[Required][RegularExpression(Grip.Utils.Consts.UserNameRegex)] string UserName,bool EmailConfirmed){
    public UserDTO():this(0,"","",false){}
};