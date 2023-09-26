using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Adapter.EverlinkProtocol.Messages
{
    public abstract class MessageBase
    {
        public byte[]? Prefix { get; set; }
        public byte[]? Data { get; set; }
        public byte[]? Suffix { get; set; }


        public static MessageBase Parse(byte[] message)
        {
            throw new NotImplementedException();
        }
        
        public byte[] Compile()
        {
            var result = new List<byte>();
            if (Prefix != null)
                result.AddRange(Prefix);
            if (Data != null)
                result.AddRange(Data);
            if (Suffix != null)
                result.AddRange(Suffix);
            return result.ToArray();
        }

    }
}