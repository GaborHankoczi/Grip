using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Grip.Middleware;

/// <summary>
/// Validates the api key in the header of the request.
/// </summary>
public class ApiKeyValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DeChunkingMiddleware> _logger;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyValidationMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware delegate.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">The configuration.</param>
    public ApiKeyValidationMiddleware(RequestDelegate next, ILogger<DeChunkingMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Invokes the API key validation middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous middleware invocation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            var attribute = endpoint?.Metadata.GetMetadata<ValidateApiKey>();
            if (attribute != null)
            {
                if (context.Request.Headers["ApiKey"] != _configuration["Station:ApiKey"])
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Please provide a valid api key");
                    return;
                }
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in ApiKeyValidationMiddleware");
        }
        await _next(context);
    }

}