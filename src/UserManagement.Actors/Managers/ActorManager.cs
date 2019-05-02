using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTracing;
using Proto;
using Proto.OpenTracing;
using Proto.Persistence;
using System;
using UserManagement.Actors.Configuration;
using UserManagement.Actors.Constants;

namespace UserManagement.Actors.Managers
{
    public class ActorManager : IActorManager
    {
        public IRootContext Context { get; }

        private readonly IActorFactory _actorFactory;

        public ActorManager(IActorFactory actorFactory, IProvider persistenceProvider, IOptions<ActorSettings> actorSettings, ITracer tracer, ILoggerFactory loggerFactory)
        {
            _actorFactory = actorFactory;
            var settings = actorSettings.Value;
            var logger = loggerFactory.CreateLogger<ActorManager>();

            // Configure OpenTracing
            Context = new RootContext(new MessageHeader(), OpenTracingExtensions.OpenTracingSenderMiddleware())
                .WithOpenTracing();

            _actorFactory.RegisterActor(new RequestActor(this, persistenceProvider, TimeSpan.FromMilliseconds(settings.ChildActorTimeoutInMilliseconds), loggerFactory, tracer), ActorNames.RequestActor);

            EventStream.Instance.Subscribe<DeadLetterEvent>(dl =>
            {
                logger.LogWarning($"DeadLetter from {dl.Sender} to {dl.Pid} : {dl.Message?.GetType().Name} = '{dl.Message?.ToString()}'");
            });
        }

        public PID GetParentActor()
        {
            return _actorFactory.GetActor<RequestActor>(ActorNames.RequestActor);
        }

        public PID GetChildActor(string id, IContext parent)
        {
            return _actorFactory.GetActor<RequestActor>(id, null, parent);
        }

        public void RegisterChildActor<T>(T actor, string id, IContext parent) where T : IActor
        {
            _actorFactory.RegisterActor(actor, id, null, parent);
        }
    }
}