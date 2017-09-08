using System.Collections.Generic;
using System.Globalization;
using Autofac;
using Collectively.Messages.Commands;
using Collectively.Common.Files;
using Collectively.Common.Mongo;
using Collectively.Common.Nancy;
using Collectively.Common.RabbitMq;
using Collectively.Common.Security;
using Collectively.Services.Users.Repositories;
using Collectively.Services.Users.Services;
using Microsoft.Extensions.Configuration;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Serilog;
using RawRabbit.Configuration;
using System.Reflection;
using Collectively.Common.Exceptionless;
using Collectively.Common.Services;
using Nancy;
using Nancy.Configuration;
using Newtonsoft.Json;
using Collectively.Common.Extensions;
using Collectively.Services.Users.Settings;
using System;
using Collectively.Messages.Events.Users;

namespace Collectively.Services.Users.Framework
{
    public class Bootstrapper : AutofacNancyBootstrapper
    {
        private static readonly ILogger Logger = Log.Logger;
        private static IExceptionHandler _exceptionHandler;
        private static readonly string DecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        private static readonly string InvalidDecimalSeparator = DecimalSeparator == "." ? "," : ".";
        private readonly IConfiguration _configuration;
        public static ILifetimeScope LifetimeScope { get; private set; }

        public Bootstrapper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

#if DEBUG
        public override void Configure(INancyEnvironment environment)
        {
            base.Configure(environment);
            environment.Tracing(enabled: false, displayErrorTraces: true);
        }
#endif

        protected override void ConfigureApplicationContainer(ILifetimeScope container)
        {
            base.ConfigureApplicationContainer(container);
            container.Update(builder =>
            {
                builder.RegisterType<CustomJsonSerializer>().As<JsonSerializer>().SingleInstance();
                builder.RegisterInstance(_configuration.GetSettings<MongoDbSettings>());
                builder.RegisterInstance(_configuration.GetSettings<FacebookSettings>());
                builder.RegisterInstance(_configuration.GetSettings<AppSettings>());
                builder.RegisterInstance(AutoMapperConfig.InitializeMapper());
                builder.RegisterModule<MongoDbModule>();
                builder.RegisterType<MongoDbInitializer>().As<IDatabaseInitializer>();
                builder.RegisterType<Encrypter>().As<IEncrypter>().SingleInstance();
                builder.RegisterType<OneTimeSecuredOperationRepository>().As<IOneTimeSecuredOperationRepository>().InstancePerLifetimeScope();
                builder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
                builder.RegisterType<UserSessionRepository>().As<IUserSessionRepository>().InstancePerLifetimeScope();
                builder.RegisterType<AuthenticationService>().As<IAuthenticationService>().InstancePerLifetimeScope();
                builder.RegisterType<FacebookClient>().As<IFacebookClient>().InstancePerLifetimeScope();
                builder.RegisterType<FacebookService>().As<IFacebookService>().InstancePerLifetimeScope();
                builder.RegisterType<OneTimeSecuredOperationService>().As<IOneTimeSecuredOperationService>().InstancePerLifetimeScope();
                builder.RegisterType<PasswordService>().As<IPasswordService>().InstancePerLifetimeScope();
                builder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
                builder.RegisterType<AvatarService>().As<IAvatarService>().InstancePerLifetimeScope();
                builder.RegisterType<Handler>().As<IHandler>();
                builder.RegisterInstance(_configuration.GetSettings<ExceptionlessSettings>()).SingleInstance();
                builder.RegisterType<ExceptionlessExceptionHandler>().As<IExceptionHandler>().SingleInstance();
                RegisterResourceFactory(builder);
                builder.RegisterModule(new FilesModule(_configuration));

                var assembly = typeof(Startup).GetTypeInfo().Assembly;
                builder.RegisterAssemblyTypes(assembly).AsClosedTypesOf(typeof(ICommandHandler<>)).InstancePerLifetimeScope();

                SecurityContainer.Register(builder, _configuration);
                RabbitMqContainer.Register(builder, _configuration.GetSettings<RawRabbitConfiguration>());
            });
            LifetimeScope = container;
        }

        protected override void RequestStartup(ILifetimeScope container, IPipelines pipelines, NancyContext context)
        {
            pipelines.SetupTokenAuthentication(container.Resolve<IJwtTokenHandler>());
            pipelines.OnError.AddItemToEndOfPipeline((ctx, ex) =>
            {
                _exceptionHandler.Handle(ex, ctx.ToExceptionData(),
                    "Request details", "Collectively", "Service", "Users");

                return ctx.Response;
            });
        }

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            var databaseSettings = container.Resolve<MongoDbSettings>();
            var databaseInitializer = container.Resolve<IDatabaseInitializer>();
            databaseInitializer.InitializeAsync();

            pipelines.BeforeRequest += (ctx) =>
            {
                FixNumberFormat(ctx);

                return null;
            };
            pipelines.AfterRequest += (ctx) =>
            {
                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                ctx.Response.Headers.Add("Access-Control-Allow-Methods", "POST,PUT,GET,OPTIONS,DELETE");
                ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization, Origin, X-Requested-With, Content-Type, Accept");
            };
            _exceptionHandler = container.Resolve<IExceptionHandler>();
            Logger.Information("Collectively.Services.Users API has started.");
        }

        private void FixNumberFormat(NancyContext ctx)
        {
            if (ctx.Request.Query == null)
                return;

            var fixedNumbers = new Dictionary<string, double>();
            foreach (var key in ctx.Request.Query)
            {
                var value = ctx.Request.Query[key].ToString();
                if (!value.Contains(InvalidDecimalSeparator))
                    continue;

                var number = 0;
                if (int.TryParse(value.Split(InvalidDecimalSeparator[0])[0], out number))
                    fixedNumbers[key] = double.Parse(value.Replace(InvalidDecimalSeparator, DecimalSeparator));
            }
            foreach (var fixedNumber in fixedNumbers)
            {
                ctx.Request.Query[fixedNumber.Key] = fixedNumber.Value;
            }
        }

        private void RegisterResourceFactory(ContainerBuilder builder)
        {
            var resources = new Dictionary<Type, string>
            {
                [typeof(SignedUp)] = "users/{0}"
            };
            builder.RegisterModule(new ResourceFactory.Module(resources));
        }
    }
}