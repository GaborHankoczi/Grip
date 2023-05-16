using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.Providers;

/// <summary>
/// Generates and validates HMAC tokens for stations.
/// </summary>
public class HMACTokenProvider : IStationTokenProvider
{
    /// <summary>
    /// Generates a token based on a key and a message
    /// </summary>
    /// <param name="key">The key to use for generating the token.</param>
    /// <param name="message">The message to use for generating the token.</param>
    /// <returns>The generated token.</returns>
    public string GenerateToken(string key, string message)
    {
        var encoding = new System.Text.ASCIIEncoding();
        byte[] keyByte = encoding.GetBytes(key);
        byte[] messageBytes = encoding.GetBytes(message);
        using (var hmacsha256 = new System.Security.Cryptography.HMACSHA256(keyByte))
        {
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashmessage);
        }
    }

    /// <summary>
    /// Validates a token based on a key and a message
    /// </summary>
    /// <param name="key">The key to use for validating the token.</param>
    /// <param name="message">The message to use for validating the token.</param>
    /// <param name="token">The token to validate.</param>
    /// <returns>True if the token is valid, false otherwise.</returns>
    public bool ValidateToken(string key, string message, string token)
    {
        var encoding = new System.Text.ASCIIEncoding();
        byte[] keyByte = encoding.GetBytes(key);
        byte[] messageBytes = encoding.GetBytes(message);
        using (var hmacsha256 = new System.Security.Cryptography.HMACSHA256(keyByte))
        {
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashmessage) == token;
        }
    }

}