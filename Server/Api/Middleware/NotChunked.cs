using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Middleware;

/// <summary>
/// Attribute for marking a controller method write response in a not chunked manner.
/// </summary>
public class NotChunked : Attribute
{

}