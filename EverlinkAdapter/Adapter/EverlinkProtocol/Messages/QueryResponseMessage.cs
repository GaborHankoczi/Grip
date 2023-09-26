using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adapter.EverlinkProtocol.Messages
{
    public class QueryResponseMessage : MessageBase
    {
        public string ResponseFileName { get; private set; }

        public QueryResponseMessage(byte[] message)
        {
            ResponseFileName = Encoding.UTF8.GetString(message).Trim('\0');
        }
    }
}