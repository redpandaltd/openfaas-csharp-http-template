using Microsoft.AspNetCore.Routing;

namespace root
{
    internal interface IRouteMatcher
    {
        RouteValueDictionary Match( string routeTemplate, string requestPath );
    }
}
