using System.Diagnostics;

namespace Portfolio.Api.Middleware;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";
        if (string.Equals(path, "/logs/stream", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var method = context.Request.Method;

        logger.LogInformation("HTTP {RequestMethod} {RequestPath} started", method, path);

        var startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            await next(context);

            var elapsedMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
            logger.LogInformation(
                "HTTP {RequestMethod} {RequestPath} finished with {StatusCode} in {Elapsed:0.0000} ms",
                method,
                path,
                context.Response.StatusCode,
                elapsedMs);
        }
        catch (Exception ex)
        {
            var elapsedMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
            logger.LogError(
                ex,
                "HTTP {RequestMethod} {RequestPath} failed in {Elapsed:0.0000} ms",
                method,
                path,
                elapsedMs);
            throw;
        }
    }
}
