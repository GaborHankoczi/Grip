using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adapter.EverlinkProtocol.Messages
{
    public class BinaryMessage : MessageBase
    {
        internal static BinaryMessage HANDSHAKE { get => new BinaryMessage(new byte[]{0x1b});}
        internal static BinaryMessage KEEPALIVE { get => new BinaryMessage(new byte[]{0x1a});}

        public BinaryMessage(byte[] message)
        {
            Prefix = new byte[]{};
            Data = message;
            Suffix = new byte[]{};
        }
    }
}