using Microsoft.Extensions.Logging;
using OpenTracing;
using Proto;
using Proto.Persistence;
using System;
using System.Threading.Tasks;
using UserManagement.Actors.Constants;
using UserManagement.Actors.Managers;
using UserManagement.Commands;
using UserManagement.Events;

namespace UserManagement.Actors
{
    public sealed class RequestActor : IActor
    {
        private readonly IActorManager _actorManager;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IProvider _persistenceProvider;
        private readonly TimeSpan _childActorTimeout;
        private readonly ILogger<RequestActor> _logger;
        private readonly ITracer _tracer;

        public RequestActor(IActorManager actorManager, IProvider persistenceProvider, TimeSpan childActorTimeout, ILoggerFactory loggerFactory, ITracer tracer)
        {
            _actorManager = actorManager;
            _loggerFactory = loggerFactory;
            _tracer = tracer;
            _persistenceProvider = persistenceProvider;
            _childActorTimeout = childActorTimeout;
            _logger = loggerFactory.CreateLogger<RequestActor>();

            _logger.LogInformation($"{nameof(RequestActor)} created");
        }

        public async Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
                _logger.LogInformation($"{nameof(RequestActor)} ID='{context.Self.Id}' has started.");
                _actorManager.RegisterChildActor(new UserActor(_persistenceProvider, ActorNames.UserActor, _tracer, _loggerFactory), ActorNames.UserActor, context);
            }
            if (context.Message is UserMessage message)
            {
                var userActor = _actorManager.GetChildActor(ActorNames.UserActor, context);
                var userEvent = await context.RequestAsync<UserEvent>(userActor, message, _childActorTimeout);
                context.Respond(userEvent);
            }
        }
    }
}