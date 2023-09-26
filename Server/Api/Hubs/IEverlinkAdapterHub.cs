using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Api.Hubs
{
    public interface IEverlinkAdapterHub
    {
        public void SendQuerryResult(byte[] result);
    }
}