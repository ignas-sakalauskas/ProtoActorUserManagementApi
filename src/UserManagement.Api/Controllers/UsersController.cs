using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenTracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Actors.Managers;
using UserManagement.Api.Configuration;
using UserManagement.Api.Constants;
using UserManagement.Api.Models.Requests;
using UserManagement.Api.Models.Responses;
using UserManagement.Commands;
using UserManagement.Events;

namespace UserManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly IActorManager _actorManager;
        private readonly ApiSettings _apiSettings;
        private readonly ITracer _tracer;

        public UsersController(IActorManager actorManager, IOptions<ApiSettings> apiSettings, ITracer tracer)
        {
            _actorManager = actorManager;
            _tracer = tracer;
            _apiSettings = apiSettings.Value;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] GetItemsFilterRequest filterRequest)
        {
            if (filterRequest == null)
                return GenericInvalidOperation(StatusCodes.Status422UnprocessableEntity, ExternalErrorReason.UnexpectedError);

            try
            {
                var limit = filterRequest.Limit ?? _apiSettings.DefaultNumberOfResults;
                var skip = filterRequest.Skip ?? _apiSettings.DefaultNumberOfSkip;

                _tracer.ActiveSpan?.Log(new Dictionary<string, object> { [nameof(limit)] = limit, [nameof(skip)] = skip, });

                var @event = await _actorManager.Context.RequestAsync<UserEvent>(
                    _actorManager.GetParentActor(),
                    new GetUsers(limit, skip),
                    TimeSpan.FromMilliseconds(_apiSettings.RequestTimeoutInMilliseconds));

                switch (@event)
                {
                    case UsersRetrieved e: return Ok(new UsersListResponse(e.TotalCount, e.Users.Select(u => new UserDetailsResponse(u.Id, u.Name, u.CreatedOn))));
                    default: return GenericInvalidOperation(StatusCodes.Status400BadRequest, ExternalErrorReason.UnexpectedError);
                }
            }
            catch (TimeoutException)
            {
                return GenericInvalidOperation(StatusCodes.Status504GatewayTimeout, ExternalErrorReason.RequestTimedOut);
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                _tracer.ActiveSpan?.Log(new Dictionary<string, object> { [nameof(id)] = id });

                var @event = await _actorManager.Context.RequestAsync<UserEvent>(
                    _actorManager.GetParentActor(),
                    new GetUser(id),
                    TimeSpan.FromMilliseconds(_apiSettings.RequestTimeoutInMilliseconds));

                switch (@event)
                {
                    case UserRetrieved e: return Ok(new UserDetailsResponse(e.Id, e.Name, e.CreatedOn));
                    case UserNotFound _: return UserNotFound();
                    default: return GenericInvalidOperation(StatusCodes.Status400BadRequest, ExternalErrorReason.UnexpectedError);
                }
            }
            catch (TimeoutException)
            {
                return GenericInvalidOperation(StatusCodes.Status504GatewayTimeout, ExternalErrorReason.RequestTimedOut);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateUserRequest request)
        {
            if (request == null)
                return GenericInvalidOperation(StatusCodes.Status422UnprocessableEntity, ExternalErrorReason.UnexpectedError);

            try
            {
                _tracer.ActiveSpan?.Log(new Dictionary<string, object> { [nameof(request)] = JsonConvert.SerializeObject(request), });

                var @event = await _actorManager.Context.RequestAsync<UserEvent>(
                    _actorManager.GetParentActor(),
                      new CreateUser(Guid.NewGuid(), request.Name),
                      TimeSpan.FromMilliseconds(_apiSettings.RequestTimeoutInMilliseconds));

                switch (@event)
                {
                    case UserCreated e: return Created(string.Empty, new UserDetailsResponse(e.Id, e.Name, e.CreatedOn));
                    default: return GenericInvalidOperation(StatusCodes.Status400BadRequest, ExternalErrorReason.UnexpectedError);
                }
            }
            catch (TimeoutException)
            {
                return GenericInvalidOperation(StatusCodes.Status504GatewayTimeout, ExternalErrorReason.RequestTimedOut);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                _tracer.ActiveSpan?.Log(new Dictionary<string, object> { [nameof(id)] = id });

                var @event = await _actorManager.Context.RequestAsync<UserEvent>(
                    _actorManager.GetParentActor(),
                     new DeleteUser(id),
                     TimeSpan.FromMilliseconds(_apiSettings.RequestTimeoutInMilliseconds));

                switch (@event)
                {
                    case UserDeleted _: return NoContent();
                    case UserNotFound _: return UserNotFound();
                    default: return GenericInvalidOperation(StatusCodes.Status400BadRequest, ExternalErrorReason.UnexpectedError);
                }
            }
            catch (TimeoutException)
            {
                return GenericInvalidOperation(StatusCodes.Status504GatewayTimeout, ExternalErrorReason.RequestTimedOut);
            }
        }
    }
}
