using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.Exceptions
{
    /// <summary>
    /// Exception thrown when a db update failed due to concurrency: HTTP 409.
    /// </summary>
    public class DbConcurrencyException : Exception
    {
        /// <summary>
        /// Creates an instance of the exception.
        /// </summary>
        /// <param name="message">Descaription of the cause of the exception</param>
        public DbConcurrencyException(string? message = null) : base(message)
        {
        }
    }
}