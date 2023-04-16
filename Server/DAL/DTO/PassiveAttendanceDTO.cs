using System.ComponentModel.DataAnnotations;

namespace Grip.DAL.DTO
{
    public record PassiveAttendanceDTO([Required] int StationId, [Required] Int64 SerialNumber);
}