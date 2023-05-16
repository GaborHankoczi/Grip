using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.Exceptions
{
    /// <summary>
    /// Exception thrown when a request is invalid: HTTP 400.
    /// </summary>
    public class BadRequestException : Exception
    {
        /// <summary>
        /// Creates an instance of the exception.
        /// </summary>
        /// <param name="message">Descaription of the cause of the exception</param>
        public BadRequestException(string? message = null) : base(message)
        {
        }

    }
}