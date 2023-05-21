using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grip.Bll.DTO;

namespace Grip.Bll.DTO;

/// <summary>
/// Represents the data transfer object for a student attendance.
/// </summary>
/// <param name="User">Info about the user</param>
/// <param name="Attendances">List of attendances</param>
public record StudentAttendanceDTO(
    UserInfoDTO User,
    IEnumerable<AttendanceDTO> Attendances);