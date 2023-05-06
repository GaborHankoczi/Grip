using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO
{
    public record ActiveAttendanceDTO([Required][RegularExpression(@"^\d*_\d*_\d*$")] string Message,[Required] string Token);
}