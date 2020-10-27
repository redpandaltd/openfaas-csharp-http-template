using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace root
{
    internal static class HttpMethodAttributeExtensions
    {
        public static HttpMethodAttribute GetMethod( this IEnumerable<HttpMethodAttribute> httpMethodAttributes, string httpMethod )
        {
            var attributeName = $"http{httpMethod}attribute";

            return httpMethodAttributes.SingleOrDefault( x => x.GetType().Name.Equals( attributeName, StringComparison.OrdinalIgnoreCase ) );
        }

        public static bool MatchRouteTemplate( this HttpMethodAttribute httpMethodAttribute, HttpContext context, IRouteMatcher routeMatcher )
        {
            if ( string.IsNullOrEmpty( httpMethodAttribute.Template ) )
            {
                return ( false );
            }

            var match = routeMatcher.Match( httpMethodAttribute.Template, context.Request.Path );

            if ( match == null )
            {
                // path doesn't match route template
                return ( false );
            }

            // write route data
            var routeData = context.GetRouteData();

            foreach ( var item in match )
            {
                routeData.Values.Add( item.Key, item.Value );
            }

            return ( true );
        }
    }
}
