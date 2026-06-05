namespace ClarityCare.Api.Middleware;

/// <summary>
/// Ensures every HTTP request/response carries a unique Correlation ID for distributed tracing.
/// If the Angular frontend sends X-Correlation-Id, it is preserved.
/// Otherwise, a new one is generated server-side.
/// The ID is propagated to the response headers for client-side correlation.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract or generate correlation ID
        if (!context.Request.Headers.TryGetValue(HeaderName, out var correlationId)
            || string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
            context.Request.Headers[HeaderName] = correlationId;
        }

        // Set on trace identifier for structured logging
        context.TraceIdentifier = correlationId!;

        // Propagate to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
