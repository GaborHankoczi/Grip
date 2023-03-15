using System;
using System.ComponentModel.DataAnnotations;

namespace Grip.DAL.DTO;

public record RegisterUserDTO([Required] string Email,[Required] string Name);
