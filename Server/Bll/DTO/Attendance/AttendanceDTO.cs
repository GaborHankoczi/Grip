using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.DTO;

/// <summary>
/// A record that represents a users attendance at a class, or lack thereof
/// </summary>
/// <param name="Class">ClassDTO of the class in question</param>
/// <param name="Present">Whether the user was present at the class</param>
/// <param name="HasExempt">Whether the user has an exempt for the class</param>
/// <param name="AuthenticationTime">Scan time if Present is true, null otherwise</param>
public record AttendanceDTO(ClassDTO Class, bool HasExempt, bool Present, DateTime? AuthenticationTime);