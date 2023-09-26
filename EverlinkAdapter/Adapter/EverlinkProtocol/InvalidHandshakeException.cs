using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adapter.EverlinkProtocol
{
    public class InvalidHandshakeException : Exception
    {
        public InvalidHandshakeException(string? message) : base(message)
        {
        }
    }
}