using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string? message = null) : base(message)
        {
        }
        
    }
}