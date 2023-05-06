using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grip.Bll.DTO;

namespace Grip.Bll.Services.Interfaces
{
    public interface IStationService
    {
        public Task<StationSecretKeyDTO> GetSecretKey(int StationNumber);
    }
}