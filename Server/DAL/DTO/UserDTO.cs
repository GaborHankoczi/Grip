using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.DAL.DTO;

public record UserDTO(string Id, string Email, string Name,bool EmailConfirmed);