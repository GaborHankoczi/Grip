using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string? message = null) : base(message)
        {
        }
    }
}