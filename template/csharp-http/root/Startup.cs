using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Redpanda.OpenFaaS;

namespace template
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors( options =>
            {
                options.AddPolicy( "AllowAll", p => p
                       .AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader() );
            } );
            
            services.AddRouting( options =>
            {
                options.LowercaseUrls = true;
            } );

            services.AddMvc()
                .AddJsonOptions( options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                } );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run( async context => {

                if ( context.Request.Path != "/" )
                {
                    context.Response.StatusCode = 404;

                    await context.Response.WriteAsync( "404" );
                    return;                
                }

                try
                {
                    IHttpFunction function = new OpenFaaS.Function();

                    var result = await function.HandleAsync( context.Request );

                    var actionContext = new ActionContext( context, new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor() );

                    await result.ExecuteResultAsync( actionContext );
                }
                catch ( NotImplementedException ex )
                {
                    context.Response.StatusCode = 501;
                    await context.Response.WriteAsync( ex.ToString() );
                }
                catch ( Exception ex )
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync( ex.ToString() );
                }

            } );
        }
    }
}
