using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Bll.Providers
{
    public interface ICurrentTimeProvider
    {
        public DateTime Now { get; }
    }
}