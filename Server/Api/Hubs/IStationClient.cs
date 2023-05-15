using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grip.Bll.DTO;

namespace Grip.Api.Hubs
{
    public interface IStationClient
    {
        public Task ReceiveScan(StationScanDTO scanDTO);
    }
}