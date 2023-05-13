using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.Providers;
public interface IStationTokenProvider
{
    /// <summary>
    /// Generates a token based on a key and a message
    /// </summary>
    public string GenerateToken(string key, string message);

    /// <summary>
    /// Generates a token based on a key and a message
    /// </summary>
    public bool ValidateToken(string key, string message, string token);
}