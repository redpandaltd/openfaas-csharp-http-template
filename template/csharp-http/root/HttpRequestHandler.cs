using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redpanda.OpenFaaS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace root
{
    internal class HttpRequestHandler
    {
        private readonly IHttpFunction function;
        private readonly HttpFunctionOptions options;
        private readonly ILogger log;

        public HttpRequestHandler( ILoggerFactory loggerFactory, IHttpFunction httpFunction, IOptions<HttpFunctionOptions> optionsAccessor )
        {
            log = loggerFactory.CreateLogger<HttpRequestHandler>();
            function = httpFunction;
            options = optionsAccessor.Value;
        }

        public async Task HandleAsync( HttpContext context )
        {
            // reject path unless allowed
            if ( !options.AllowCustomPath && ( context.Request.Path != "/" ) )
            {
                context.Response.StatusCode = 404;

                await context.Response.WriteAsync( "Not Found" );

                log.LogInformation( $"{context.Request.Method} {context.Request.Path}  404" );

                return;
            }

            try
            {
                // get function http modifiers
                var methodInfo = function.GetType().GetMethod( "HandleAsync" );
                var httpAttributes = methodInfo.GetCustomAttributes( typeof( Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute ), false );

                if ( httpAttributes.Any() )
                {
                    // we have http modifiers, let's match with the request method
                    var attributeName = $"http{context.Request.Method}attribute";

                    if ( !httpAttributes.Any( x => x.GetType().Name.Equals( attributeName, StringComparison.OrdinalIgnoreCase ) ) )
                    {
                        // method not allowed
                        context.Response.StatusCode = 405;

                        await context.Response.WriteAsync( "Method Not Allowed" );
                        return;
                    }
                }

                // execute function
                var result = await function.HandleAsync( context.Request );

                // write result
                var actionContext = new ActionContext( context, new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor() );

                await result.ExecuteResultAsync( actionContext );
            }
            catch ( NotImplementedException ex )
            {
                context.Response.StatusCode = 501;

                await context.Response.WriteAsync( ex.ToString() );

                log.LogError( ex, ex.Message );
            }
            catch ( Exception ex )
            {
                context.Response.StatusCode = 500;

                await context.Response.WriteAsync( ex.ToString() );

                log.LogError( ex, ex.Message );
            }
        }
    }
}
