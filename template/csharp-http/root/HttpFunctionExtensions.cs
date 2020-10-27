using Microsoft.AspNetCore.Mvc.Routing;
using Redpanda.OpenFaaS;
using System.Linq;

namespace root
{
    internal static class HttpFunctionExtensions
    {
        public static HttpMethodAttribute[] GetHttpMethodAttributes( this IHttpFunction function )
        {
            var methodInfo = function.GetType().GetMethod( "HandleAsync" );
            var httpAttributes = methodInfo.GetCustomAttributes( typeof( HttpMethodAttribute ), false );

            return httpAttributes.Cast<HttpMethodAttribute>().ToArray();
        }
    }
}
