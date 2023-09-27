using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.Services.Interfaces
{
    public interface IEverlinkAdapterService
    {
        public void ResponseReceived(byte[] response);
        public Task<byte[]> SendQuery(string quary);
        public bool IsConnected();

        public void AdapterConnected();
        public void AdapterDisconnected();
    }
}