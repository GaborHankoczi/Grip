using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grip.Bll.DTO;

namespace Grip.Bll.DTO;
public record StudentAttendanceDTO(
    UserInfoDTO User,
    IEnumerable<AttendanceDTO> Attendances);