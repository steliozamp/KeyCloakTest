using Microsoft.Extensions.Primitives;

namespace KeyCloakTest.Infrastructure;

public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context.Request.Headers);
        context.TraceIdentifier = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["correlationId"] = correlationId
               }))
        {
            await next(context);
        }
    }

    private static string ResolveCorrelationId(IHeaderDictionary headers)
    {
        if (headers.TryGetValue(HeaderName, out StringValues requested) && !StringValues.IsNullOrEmpty(requested))
        {
            return requested.ToString();
        }

        return Guid.NewGuid().ToString("N");
    }
}
