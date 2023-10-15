using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adapter.EverlinkProtocol.Messages
{
    public class FileResponseMessage : MessageBase
    {
        public bool IsReady { get; private set; }
        public byte[]? FileContent { get; private set; }
        public FileResponseMessage(byte[] message)
        {
            if(message.Length == 4 && message.SequenceEqual(new byte[]{0x00,0x00,0x00,0x00}))
            {
                IsReady = false;                
            }
            else
            {
                FileContent = message;
                IsReady = true;
            }
        }
    }
}