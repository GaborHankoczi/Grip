using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Grip.Middleware;

/// <summary>
/// Middleware to de-chunk HTTP requests, because ucontroller doesn't support chunked responses
/// </summary>
public class DeChunkingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DeChunkingMiddleware> _logger;
    public DeChunkingMiddleware(RequestDelegate next, ILogger<DeChunkingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try{
            var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            var attribute = endpoint?.Metadata.GetMetadata<NotChunked>();
            if(attribute != null)
            {
                var originalBodyStream = context.Response.Body;
                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;
                    long length = 0;
                    context.Response.OnStarting(() =>
                    {
                        context.Response.Headers.ContentLength = length;
                        return Task.CompletedTask;
                    });
                    await _next(context);

                    // If you want to read the body, uncomment these lines.
                    //context.Response.Body.Seek(0, SeekOrigin.Begin);
                    //var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

                    length = context.Response.Body.Length;
                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }else{
                await _next(context);
            }
        }catch(Exception e){
            _logger.LogError(e,"Error in DeChunkingMiddleware");
            await _next(context);
        }
    }
}