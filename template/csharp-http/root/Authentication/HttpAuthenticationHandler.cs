using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Redpanda.OpenFaaS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace root
{
    internal class HttpAuthenticationHandler
    {
        private readonly ILogger logger;
        private readonly IAuthenticationService authService;
        private readonly IAuthenticationSchemeProvider schemeProvider;

        public HttpAuthenticationHandler( ILoggerFactory loggerFactory
            , IAuthenticationService authenticationService
            , IAuthenticationSchemeProvider authenticationSchemeProvider )
        {
            logger = loggerFactory.CreateLogger<HttpAuthenticationHandler>();
            authService = authenticationService;
            schemeProvider = authenticationSchemeProvider;
        }

        public async Task AuthenticateAsync( HttpContext context )
        {
            var defaultScheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync();
            var schemes = await schemeProvider.GetAllSchemesAsync();

            if ( defaultScheme != null )
            {
                schemes = new AuthenticationScheme[]
                {
                            defaultScheme
                }
                .Union( schemes.Except( new AuthenticationScheme[] { defaultScheme } ) )
                .ToArray();
            }

            foreach ( var scheme in schemes )
            {
                var authResult = await authService.AuthenticateAsync( context, scheme.Name );

                if ( authResult.Succeeded )
                {
                    context.User = authResult.Principal;
                    break;
                }
            }
        }
    }
}
