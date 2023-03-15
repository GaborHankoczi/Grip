using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Services;

public class StartupService : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        throw new NotImplementedException();
    }
}
