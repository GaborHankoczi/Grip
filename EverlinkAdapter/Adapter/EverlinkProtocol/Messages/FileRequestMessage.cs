using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adapter.EverlinkProtocol.Messages
{
    public class FileRequestMessage : MessageBase
    {
        public FileRequestMessage(string fileName)
        {
            Prefix = new byte[]{0x13};
            Data = Encoding.UTF8.GetBytes($".\\{fileName}");
            Suffix = new byte[]{0x00};
        }
    }
}