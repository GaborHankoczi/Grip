using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Middleware;

/// <summary>
/// Attribute for marking a controller method as requiring a valid API key.
/// </summary>
public class ValidateApiKey : Attribute
{

}