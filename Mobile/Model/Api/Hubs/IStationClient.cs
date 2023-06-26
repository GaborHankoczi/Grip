using GripMobile.Model.Api.HubDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GripMobile.Model.Api.Hubs
{
    /// <summary>
    /// This interface is used to define the methods that can be called from the station hub on the client
    /// </summary>
    public interface IStationClient
    {
        /// <summary>
        /// This method is called when an attendance is registered
        /// </summary>
        /// <param name="scanDTO">DTO containing information about the scan</param>
        public Task ReceiveScan(StationScanDTO scanDTO);
    }
}
