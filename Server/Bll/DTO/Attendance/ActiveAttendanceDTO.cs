using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO
{
    /// <summary>
    /// DTO for the active attendance registration
    /// </summary>
    /// <param name="Message">Message provided by the station</param>
    /// <param name="Token">Message signed by the station</param>
    public record ActiveAttendanceDTO([Required][RegularExpression(@"^\d*_\d*_\d*$")] string Message, [Required] string Token);
}