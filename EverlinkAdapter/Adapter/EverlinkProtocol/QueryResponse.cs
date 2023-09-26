using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adapter.EverlinkProtocol
{
    public class QueryResponse
    {
        public bool IsComplete { get; private set; } = false;

        private List<byte> _buffer = new List<byte>();
        public byte[] Data { get => _buffer.ToArray(); }

        public void Append(byte[] data)
        {
            _buffer.AddRange(data);
            if(data.Skip(data.Length-4).Take(4).SequenceEqual(new byte[]{0x00,0x00,0x00,0x00}))
                IsComplete = true;
        }
    }
}