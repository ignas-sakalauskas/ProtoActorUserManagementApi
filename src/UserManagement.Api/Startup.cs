using Jaeger;
using Jaeger.Samplers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;
using Proto;
using Proto.OpenTracing;
using Proto.Persistence;
using UserManagement.Actors;
using UserManagement.Actors.Configuration;
using UserManagement.Actors.Managers;
using UserManagement.Api.Configuration;
using UserManagement.Api.Logging;
using UserManagement.Api.Monitoring;
using UserManagement.Persistence;

namespace UserManagement.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            services.AddOptions();
            services.Configure<ApiSettings>(_configuration.GetSection("Api"));
            services.Configure<ActorSettings>(_configuration.GetSection("Actors"));

            // Logging
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(_configuration.GetSection("Logging"));
                builder.AddSeq(_configuration.GetSection("Seq"));
                builder.AddDebug();
            });

            // Persistence
            services.AddSingleton<IProvider, InMemoryProvider>();

            // Monitoring
            services.AddSingleton<ITracer>(Jaeger.Configuration.FromIConfiguration(_loggerFactory, _configuration.GetSection("Jaeger")).GetTracer());

            // Actors
            services.AddSingleton<IActorManager, ActorManager>();
            services.AddProtoActor(props =>
            {
                props.RegisterProps<RequestActor>(p => p.WithChildSupervisorStrategy(new AlwaysRestartStrategy()).WithOpenTracing());
                props.RegisterProps<UserActor>(p => p.WithOpenTracing());
            });

            // API
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, ITracer tracer)
        {
            _loggerFactory.AddDebug();
            Log.SetLoggerFactory(_loggerFactory);

            // Register Jaeger monitoring
            GlobalTracer.Register(tracer);

            app.UseForwardedHeaders();
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<RequestMonitoringMiddleware>();
            app.UseMiddleware<ChaosMiddleware>();
            app.UseMvc();
        }
    }
}
