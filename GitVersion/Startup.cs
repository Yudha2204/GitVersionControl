using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GitVersion
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile($"appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore(opt => opt.EnableEndpointRouting = false);

            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var options = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                RequireHeaderSymmetry = false
            };
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

            app.UseForwardedHeaders(options);

            app.UseCors(builder => builder
               .AllowAnyHeader()
               .AllowAnyMethod()
               .SetIsOriginAllowed(origin => true)
               .AllowCredentials());

            app.UseExceptionHandler(
                builder =>
                {
                    builder.Run(
                    async context =>
                    {
                        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        context.Response.ContentType = "application/json";
                        var ex = context.Features.Get<IExceptionHandlerFeature>();
                        if (ex != null)
                        {
                            await context.Response.Body.WriteAsync(Encoding.ASCII.GetBytes(ex.Error.Message), 0, ex.Error.Message.Length).ConfigureAwait(false);
                        }
                    });
                }

            );

            app.UseMvc();
        }
    }
}
