using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenTracing;
using Proto;
using Proto.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagement.Actors.Chaos;
using UserManagement.Commands;
using UserManagement.Domain;
using UserManagement.Events;

namespace UserManagement.Actors
{
    public class UserActor : IActor
    {
        private readonly Persistence _persistence;
        private readonly ITracer _tracer;
        private readonly ILogger<UserActor> _logger;

        private Users _state = new Users();

        public UserActor(IProvider provider, string actorId, ITracer tracer, ILoggerFactory loggerFactory)
        {
            _tracer = tracer;
            _logger = loggerFactory.CreateLogger<UserActor>();
            _persistence = Persistence.WithEventSourcingAndSnapshotting(provider, provider, actorId, ApplyEvent, ApplySnapshot);

            _logger.LogInformation($"{nameof(UserActor)} ID='{actorId}' created");
        }

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                    {
                        _logger.LogInformation($"{nameof(UserActor)} ID='{context.Self.Id}' has started");
                        using (_tracer.BuildSpan("Recovering actor state").StartActive())
                        {
                            await _persistence.RecoverStateAsync();
                        }
                    }
                    break;
                case GetUsers msg:
                    {
                        var @event = _state.GetAllUsers(msg.Limit, msg.Skip);
                        context.Respond(@event);
                    }
                    break;

                case GetUser msg:
                    {
                        var @event = _state.GetUserById(msg.Id);
                        context.Respond(@event);
                    }
                    break;

                case CreateUser msg:
                    {
                        try
                        {
                            using (var scope = _tracer.BuildSpan("Persisting UserCreated event").StartActive())
                            {
                                if (ChaosHelper.ShouldCreateUserCrash(scope.Span))
                                    throw new Exception("Creating user disabled by Chaos");

                                var @event = _state.CreateUser(msg.Id, msg.Name);
                                if (@event is UserCreated)
                                {

                                    scope.Span.Log(new Dictionary<string, object> { [nameof(@event)] = JsonConvert.SerializeObject(@event) });

                                    await _persistence.PersistEventAsync(@event)
                                        .ContinueWith(t => _persistence.PersistSnapshotAsync(_state));
                                }
                                context.Respond(@event);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Error creating user");
                            context.Respond(new UnexpectedErrorOcurred(msg.Id));
                        }
                    }
                    break;

                case DeleteUser msg:
                    {
                        try
                        {
                            var @event = _state.DeleteUser(msg.Id);
                            if (@event is UserDeleted)
                            {
                                using (var scope = _tracer.BuildSpan("Persisting UserDeleted event").StartActive())
                                {
                                    scope.Span.Log(new Dictionary<string, object> { [nameof(@event)] = JsonConvert.SerializeObject(@event), });

                                    await _persistence.PersistEventAsync(@event)
                                        .ContinueWith(t => _persistence.PersistSnapshotAsync(_state));
                                }
                            }
                            context.Respond(@event);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Error deleting user");
                            context.Respond(new UnexpectedErrorOcurred(msg.Id));
                        }
                    }
                    break;
            }
        }

        private void ApplyEvent(Event @event)
        {
            switch (@event.Data)
            {
                case UserCreated e:
                    _state.CreateUser(e.Id, e.Name, e.CreatedOn);
                    break;

                case UserDeleted e:
                    _state.DeleteUser(e.Id);
                    break;
            }
        }

        private void ApplySnapshot(Snapshot snapshot)
        {
            try
            {
                _state = JsonConvert.DeserializeObject<Users>(JsonConvert.SerializeObject(snapshot.State));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving or deserializing state from a snapshot");
            }
        }
    }
}