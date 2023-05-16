using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.Exceptions
{
    /// <summary>
    /// Exception thrown when a request needs authorization: HTTP 401.
    /// </summary>
    public class UnauthorizedException : Exception
    {
        /// <summary>
        /// Creates an instance of the exception.
        /// </summary>
        /// <param name="message">Descaription of the cause of the exception</param>
        public UnauthorizedException(string? message = null) : base(message)
        {
        }
    }
}