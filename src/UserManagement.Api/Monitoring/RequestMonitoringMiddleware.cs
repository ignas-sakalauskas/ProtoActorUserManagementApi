using Microsoft.AspNetCore.Http;
using OpenTracing;
using OpenTracing.Tag;
using System;
using System.Threading.Tasks;

namespace UserManagement.Api.Monitoring
{
    public sealed class RequestMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITracer _tracer;

        public RequestMonitoringMiddleware(RequestDelegate next, ITracer tracer)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _tracer = tracer;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            using (var scope = _tracer.BuildSpan("HttpRequest").StartActive())
            {
                try
                {
                    await _next(httpContext);

                    scope.Span
                        .SetTag(Tags.SpanKind, Tags.SpanKindServer)
                        .SetTag(Tags.HttpMethod, "GET")
                        .SetTag(Tags.HttpUrl, httpContext.Request.Path)
                        .SetTag(Tags.HttpStatus, httpContext.Response?.StatusCode ?? 0);
                }
                catch (Exception)
                {
                    scope.Span.SetTag(Tags.Error, true);
                    throw;
                }
            }
        }
    }
}
