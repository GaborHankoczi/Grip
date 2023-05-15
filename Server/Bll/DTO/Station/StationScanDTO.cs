using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.DTO
{
    public record StationScanDTO
    {
        public UserInfoDTO UserInfo { get; init; }
        public DateTime ScanTime { get; init; }
        public int StationId { get; init; }
    }
}