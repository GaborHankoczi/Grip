using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Grip.Middleware;
public class ApiKeyValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DeChunkingMiddleware> _logger;
    private readonly IConfiguration _configuration;
    public ApiKeyValidationMiddleware(RequestDelegate next, ILogger<DeChunkingMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

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
            await _next(context);
        }
        await _next(context);
    }

}