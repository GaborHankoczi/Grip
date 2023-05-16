using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.Exceptions
{
    /// <summary>
    /// Exception thrown when a resource is not found: HTTP 404.
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Creates an instance of the exception.
        /// </summary>
        /// <param name="message">Descaription of the cause of the exception</param>
        public NotFoundException(string? message = null) : base(message)
        {
        }
    }
}