using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.DTO
{
    /// <summary>
    /// Represents the data transfer object for a station scan.
    /// </summary>
    public record StationScanDTO
    {
        /// <summary>
        /// Info about the user that scanned
        /// </summary>
        public UserInfoDTO UserInfo { get; init; } = null!;

        /// <summary>
        /// Time of the scan
        /// </summary>
        public DateTime ScanTime { get; init; }

        /// <summary>
        /// Id of the station where the scan occured
        /// </summary>
        public int StationId { get; init; }
    }
}