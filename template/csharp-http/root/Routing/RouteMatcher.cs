using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace root
{
    internal class RouteMatcher : IRouteMatcher
    {
        public RouteValueDictionary Match( string routeTemplate, string requestPath )
        {
            var template = TemplateParser.Parse( routeTemplate );

            var matcher = new TemplateMatcher( template, GetDefaults( template ) );

            var values = new RouteValueDictionary();

            if ( !matcher.TryMatch( requestPath, values ) )
            {
                return ( null );
            }

            return ( values );
        }

        private RouteValueDictionary GetDefaults( RouteTemplate routeTemplate )
        {
            var result = new RouteValueDictionary();

            foreach ( var parameter in routeTemplate.Parameters )
            {
                if ( parameter.DefaultValue != null )
                {
                    result.Add( parameter.Name, parameter.DefaultValue );
                }
            }

            return ( result );
        }
    }
}
