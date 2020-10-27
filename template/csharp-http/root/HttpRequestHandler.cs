﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redpanda.OpenFaaS;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace root
{
    internal class HttpRequestHandler
    {
        private readonly IHttpFunction function;
        private readonly HttpFunctionOptions options;
        private readonly IRouteMatcher routeMatcher;
        private readonly ILogger log;

        public HttpRequestHandler( ILoggerFactory loggerFactory
            , IHttpFunction httpFunction
            , IOptions<HttpFunctionOptions> optionsAccessor
            , IRouteMatcher internalRouteMatcher )
        {
            log = loggerFactory.CreateLogger<HttpRequestHandler>();
            function = httpFunction;
            options = optionsAccessor.Value;
            routeMatcher = internalRouteMatcher;
        }

        public async Task HandleAsync( HttpContext context )
        {
            try
            {
                // get function http modifiers
                var httpAttributes = function.GetHttpMethodAttributes();

                if ( httpAttributes.Any() )
                {
                    // we have http modifiers, let's match with the request method
                    var httpAttribute = httpAttributes.GetMethod( context.Request.Method );

                    if ( httpAttribute == null )
                    {
                        // method not allowed
                        await WriteMethodNotAllowedAsync( context );

                        return;
                    }

                    if ( !string.IsNullOrEmpty( httpAttribute.Template ) )
                    {
                        // match route template
                        if ( !httpAttribute.MatchRouteTemplate( context, routeMatcher ) )
                        {
                            // path doesn't match route template
                            await WriteNotFoundAsync( context );

                            return;
                        }

                    }
                    else if ( !options.AllowCustomPath && ( context.Request.Path != "/" ) )
                    {
                        // attribute template is null. reject custom path unless allowed by options
                        await WriteNotFoundAsync( context );

                        return;
                    }
                }
                else if ( !options.AllowCustomPath && ( context.Request.Path != "/" ) )
                {
                    // if there are no http modifiers, we reject a custom path unless allowed by options
                    await WriteNotFoundAsync( context );

                    return;
                }

                // execute function
                var result = await function.HandleAsync( context.Request );

                // write result
                var actionContext = new ActionContext( context, context.GetRouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor() );

                await result.ExecuteResultAsync( actionContext );
            }
            catch ( NotImplementedException ex )
            {
                await WriteExceptionAsync( context, ex, 501 );
            }
            catch ( Exception ex )
            {
                await WriteExceptionAsync( context, ex );
            }
        }

        private Task WriteNotFoundAsync( HttpContext context )
        {
            log.LogInformation( $"{context.Request.Method} {context.Request.Path}  404" );

            context.Response.StatusCode = 404;

            return context.Response.WriteAsync( "Not Found" );
        }

        private Task WriteMethodNotAllowedAsync( HttpContext context )
        {
            context.Response.StatusCode = 405;

            return context.Response.WriteAsync( "Method Not Allowed" );
        }

        private Task WriteExceptionAsync( HttpContext context, Exception ex, int statusCode = 500 )
        {
            log.LogError( ex, ex.Message );

            context.Response.StatusCode = statusCode;

            return context.Response.WriteAsync( ex.ToString() );
        }
    }
}
