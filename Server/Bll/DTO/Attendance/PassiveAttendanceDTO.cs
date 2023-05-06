using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO
{
    public record PassiveAttendanceDTO([Required] int StationId, [Required] Int64 SerialNumber);
}