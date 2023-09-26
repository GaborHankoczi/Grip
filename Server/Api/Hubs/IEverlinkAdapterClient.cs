using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Api.Hubs
{
    public interface IEverlinkAdapterClient
    {
        /// <summary>
        /// Sends a quary to the client where the client executes the quary and returns the result as a zip file
        /// </summary>
        /// <param name="query">Query to be executed</param>
        /// <returns></returns>
        public Task RunQuerry(string query);
    }
}