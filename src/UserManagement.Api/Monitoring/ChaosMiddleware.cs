using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenTracing;
using System;
using System.Threading.Tasks;
using UserManagement.Actors.Chaos;
using UserManagement.Api.Logging;

namespace UserManagement.Api.Monitoring
{
    public sealed class ChaosMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITracer _tracer;
        private readonly ILogger _logger;

        public ChaosMiddleware(RequestDelegate next, ITracer tracer, ILoggerFactory loggerFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _tracer = tracer;
            _logger = loggerFactory.CreateLogger<RequestLoggingMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            // These headers should not be allowed in public facing API :)
            httpContext.Request.Headers.TryGetValue(Headers.ChaosType, out var chaosTypeHeaderValue);

            var chaosTypeValue = chaosTypeHeaderValue.ToString().ToLowerInvariant();

            if (chaosTypeValue == Headers.CreateUserDown)
            {
                var span = _tracer.ActiveSpan;
                if (span != null)
                {
                    span.SetBaggageItem(Headers.ChaosType, chaosTypeValue);

                    _logger.LogWarning($"Attempting to enable Chaos Engineering mode with type '{chaosTypeValue}'");
                }
            }

            await _next(httpContext);
        }
    }
}
