using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grip.Bll.DTO.Everlink;

namespace Grip.Bll.Services.Interfaces
{
    public interface IEverlinkAdapterService
    {
        public void ResponseReceived(byte[] response);
        public Task<TableDTO> SendQuery(string quary);
        public Task<byte[]> SendQueryZip(string quary);
        public bool IsConnected();

        public void AdapterConnected();
        public void AdapterDisconnected();
    }
}