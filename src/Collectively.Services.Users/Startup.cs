using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nancy.Owin;
using Lockbox.Client.Extensions;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Collectively.Services.Users.Framework;
using Collectively.Common.Logging;

namespace Collectively.Services.Users
{
    public class Startup
    {
        public string EnvironmentName {get;set;}
        public IConfiguration Configuration { get; set; }
        public IContainer ApplicationContainer { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSerilog(Configuration);
            services.AddWebEncoders();
            services.AddCors();
            ApplicationContainer = GetServiceContainer(services);

            return new AutofacServiceProvider(ApplicationContainer);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseSerilog(loggerFactory);
            app.UseCors(builder => builder.AllowAnyHeader()
	            .AllowAnyMethod()
	            .AllowAnyOrigin()
	            .AllowCredentials());
            app.UseOwin().UseNancy(x => x.Bootstrapper = new Bootstrapper(Configuration));
        }

        protected static IContainer GetServiceContainer(IEnumerable<ServiceDescriptor> services)
        {
            var builder = new ContainerBuilder();
            builder.Populate(services);

            return builder.Build();
        }
    }
}