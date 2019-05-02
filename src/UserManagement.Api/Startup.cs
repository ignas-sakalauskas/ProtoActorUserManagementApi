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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            services.AddOptions();
            services.Configure<ApiSettings>(Configuration.GetSection("Api"));
            services.Configure<ActorSettings>(Configuration.GetSection("Actors"));

            // Logging
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(Configuration.GetSection("Logging"));
                builder.AddSeq(Configuration.GetSection("Seq"));
                builder.AddDebug();
            });

            // Persistence
            services.AddSingleton<IProvider, InMemoryProvider>();

            // Monitoring
            services.AddSingleton<ITracer>(new Tracer.Builder("UserManagementApi").WithSampler(new ConstSampler(true)).Build());

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

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, ITracer tracer)
        {
            loggerFactory.AddDebug();
            Log.SetLoggerFactory(loggerFactory);

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
