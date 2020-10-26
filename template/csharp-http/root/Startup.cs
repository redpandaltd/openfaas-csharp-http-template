using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using root;

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
                .AddNewtonsoftJson( options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                } );
            // Replaced with Newtonsoft because Microsoft's serializer doesn't do polymorphic serialization

            // allow function implementation to add services to the container
            new OpenFaaS.Startup().ConfigureServices( services );

            // add root request handler to the container
            services.AddTransient<HttpRequestHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var requestHandler = app.ApplicationServices.GetRequiredService<HttpRequestHandler>();

            app.Run( requestHandler.HandleAsync );
        }
    }
}
