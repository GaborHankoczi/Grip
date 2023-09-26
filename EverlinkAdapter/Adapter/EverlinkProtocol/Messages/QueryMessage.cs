using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adapter.EverlinkProtocol.Messages
{
    public class QueryMessage : MessageBase
    {
        
        public QueryMessage(string quary)
        {
            Prefix = new byte[]{0x18,0x00};
            Data = Encoding.UTF8.GetBytes(quary);
            Suffix = new byte[]{0x00};
        }
    }
}