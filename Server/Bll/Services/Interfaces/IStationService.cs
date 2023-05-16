using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grip.Bll.DTO;

namespace Grip.Bll.Services.Interfaces
{
    /// <summary>
    /// Represents an interface for the station service.
    /// </summary>
    public interface IStationService
    {
        /// <summary>
        /// Gets the secret key for a station.
        /// </summary>
        public Task<StationSecretKeyDTO> GetSecretKey(int StationNumber);
    }
}