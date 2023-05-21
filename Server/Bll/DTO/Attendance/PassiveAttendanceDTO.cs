using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO
{
    /// <summary>
    /// DTO for the passive attendance registration
    /// </summary>
    /// <param name="StationId">Id of the station of the scan</param>
    /// <param name="SerialNumber">Serial number of the passive NFC tag</param>
    public record PassiveAttendanceDTO([Required] int StationId, [Required] Int64 SerialNumber);
}