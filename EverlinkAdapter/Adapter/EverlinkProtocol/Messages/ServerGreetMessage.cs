using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adapter.EverlinkProtocol.Messages
{
    public class ServerGreetMessage : MessageBase
    {
        public ServerGreetMessage(byte[] message)
        {
            if(Encoding.UTF8.GetString(message).Trim('\0')!="+EverlinkServer\r\n")
                throw new InvalidHandshakeException("Wrong server greet message");
        }

        public new static ServerGreetMessage Parse(byte[] message)
        {
            return new ServerGreetMessage(message);
        }
    }
}