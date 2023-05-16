using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Bll.Providers
{
    /// <summary>
    /// Interface for abstracting the current time for testing purposes.
    /// </summary>
    public interface ICurrentTimeProvider
    {
        /// <summary>
        /// Current time in UTC.
        /// </summary>
        public DateTime Now { get; }
    }
}