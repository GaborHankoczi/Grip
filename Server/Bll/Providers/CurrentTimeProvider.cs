using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Bll.Providers
{
    public class CurrentTimeProvider : ICurrentTimeProvider
    {
        public DateTime Now { get => DateTime.UtcNow; }
    }
}