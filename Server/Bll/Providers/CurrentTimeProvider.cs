using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Bll.Providers
{
    /// <summary>
    /// Class for abstracting the current time for testing purposes.
    /// </summary>
    public class CurrentTimeProvider : ICurrentTimeProvider
    {
        /// <summary>
        /// Current time in UTC.
        /// </summary>
        public DateTime Now { get => DateTime.UtcNow; }
    }
}