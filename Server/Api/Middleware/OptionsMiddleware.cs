using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Grip.Api.Middleware
{
    public class OptionsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OptionsMiddleware> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware delegate.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        public OptionsMiddleware(RequestDelegate next, ILogger<OptionsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
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

                if (endpoint != null && context.Request.Method == "OPTIONS")
                {
                    var controllerActionDescriptor = endpoint
                        .Metadata
                        .GetMetadata<ControllerActionDescriptor>();
                    var methods = controllerActionDescriptor.ControllerTypeInfo.GetMethods();
                    var supportedMethods = new List<string>();
                    foreach (var method in methods)
                    {
                        var httpMethodAttributes = method.GetCustomAttributes(typeof(HttpMethodAttribute), false);
                        foreach (var httpMethodAttribute in httpMethodAttributes)
                        {
                            var attribute = httpMethodAttribute as HttpMethodAttribute;
                            if (attribute != null)
                            {
                                supportedMethods.Add(attribute.HttpMethods.First());
                            }
                        }

                    }
                    supportedMethods = supportedMethods.Distinct().ToList();
                    context.Response.Headers.Add("Allow", String.Join(", ", supportedMethods));
                    //context.Response.StatusCode = (int)HttpStatusCode.OK;
                    return;
                }

                /*var attribute = endpoint?.Metadata.GetMetadata<ValidateApiKey>();
                if (attribute != null)
                {
                    if (context.Request.Headers["ApiKey"] != _configuration["Station:ApiKey"])
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync("Please provide a valid api key");
                        return;
                    }
                }*/

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ApiKeyValidationMiddleware");
            }
            await _next(context);
        }
    }
}