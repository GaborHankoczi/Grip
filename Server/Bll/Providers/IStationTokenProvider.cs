using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.Providers;

/// <summary>
/// Interface for generating and validating tokens for stations.
/// </summary>
public interface IStationTokenProvider
{
    /// <summary>
    /// Generates a token based on a key and a message
    /// </summary>
    /// <param name="key">The key to use for generating the token.</param>
    /// <param name="message">The message to use for generating the token.</param>
    /// <returns>The generated token.</returns>
    public string GenerateToken(string key, string message);

    /// <summary>
    /// Validates a token based on a key and a message
    /// </summary>
    /// <param name="key">The key to use for validating the token.</param>
    /// <param name="message">The message to use for validating the token.</param>
    /// <param name="token">The token to validate.</param>
    /// <returns>True if the token is valid, false otherwise.</returns>
    public bool ValidateToken(string key, string message, string token);
}