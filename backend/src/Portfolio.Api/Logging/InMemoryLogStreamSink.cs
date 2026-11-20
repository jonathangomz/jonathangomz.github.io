using Serilog.Core;
using Serilog.Events;

namespace Portfolio.Api.Logging;

public sealed class InMemoryLogStreamSink(LogStreamBroadcaster broadcaster) : ILogEventSink
{
    private const string RequestMiddlewareSource = "Portfolio.Api.Middleware.RequestLoggingMiddleware";
    private const string CacheServiceSource = "Portfolio.Infrastructure.Caching.MemoryCacheService";

    public static readonly IReadOnlySet<string> AllowedRoutes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "/about",
        "/experience",
        "/stack",
        "/projects",
        "/contact",
        "/portfolio",
        "/health"
    };

    public void Emit(LogEvent logEvent)
    {
        if (!TryGetScalarString(logEvent, "SourceContext", out var sourceContext))
        {
            return;
        }

        string source;
        if (string.Equals(sourceContext, RequestMiddlewareSource, StringComparison.Ordinal))
        {
            if (!TryGetScalarString(logEvent, "RequestPath", out var requestPath) ||
                !AllowedRoutes.Contains(requestPath))
            {
                return;
            }

            source = "Request";
        }
        else if (string.Equals(sourceContext, CacheServiceSource, StringComparison.Ordinal))
        {
            if (!TryGetScalarString(logEvent, "CacheKey", out var cacheKey) ||
                !cacheKey.StartsWith("portfolio:", StringComparison.Ordinal))
            {
                return;
            }

            source = "Cache";
        }
        else
        {
            return;
        }

        var message = RenderCleanMessage(logEvent);
        var entry = new LogStreamEntry(
            logEvent.Timestamp,
            FormatLevel(logEvent.Level),
            source,
            message);

        broadcaster.Publish(entry);
    }

    private static bool TryGetScalarString(LogEvent logEvent, string propertyName, out string value)
    {
        if (logEvent.Properties.TryGetValue(propertyName, out var propertyValue) &&
            propertyValue is ScalarValue { Value: string str })
        {
            value = str;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static string RenderCleanMessage(LogEvent logEvent)
    {
        using var writer = new StringWriter();
        foreach (var token in logEvent.MessageTemplate.Tokens)
        {
            if (token is Serilog.Parsing.PropertyToken propertyToken &&
                logEvent.Properties.TryGetValue(propertyToken.PropertyName, out var propVal) &&
                propVal is ScalarValue { Value: string strVal } &&
                string.IsNullOrEmpty(propertyToken.Format))
            {
                writer.Write(strVal);
            }
            else
            {
                token.Render(logEvent.Properties, writer);
            }
        }

        return writer.ToString();
    }

    private static string FormatLevel(LogEventLevel level) => level switch
    {
        LogEventLevel.Verbose => "VRB",
        LogEventLevel.Debug => "DBG",
        LogEventLevel.Information => "INF",
        LogEventLevel.Warning => "WRN",
        LogEventLevel.Error => "ERR",
        LogEventLevel.Fatal => "FTL",
        _ => "INF"
    };
}
