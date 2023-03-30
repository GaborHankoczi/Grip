using System.ComponentModel.DataAnnotations;

namespace Grip.DAL.DTO
{
    public record AttendanceDTO([Required] Int64 ScanId, [Required] int StationId);
}