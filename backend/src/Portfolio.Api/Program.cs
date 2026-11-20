using System.Globalization;
using System.Threading.RateLimiting;
using Portfolio.Api.Endpoints;
using Portfolio.Api.Logging;
using Portfolio.Api.Middleware;
using Portfolio.Application;
using Portfolio.Infrastructure;
using Serilog;
using Serilog.Events;

const string CorsPolicyName = "PortfolioCors";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<LogStreamBroadcaster>();
builder.Services.AddSingleton<InMemoryLogStreamSink>();

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Sink(services.GetRequiredService<InMemoryLogStreamSink>());
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["https://jonathangomz.github.io", "http://localhost:4321", "http://127.0.0.1:4321"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var permitLimit = Math.Max(1, builder.Configuration.GetValue("RateLimiting:PermitLimit", 60));
var windowSeconds = Math.Max(1, builder.Configuration.GetValue("RateLimiting:WindowSeconds", 60));
var queueLimit = Math.Max(0, builder.Configuration.GetValue("RateLimiting:QueueLimit", 0));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
            context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString(CultureInfo.InvariantCulture);
        }

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests. Please try again later.",
            status = StatusCodes.Status429TooManyRequests
        }, cancellationToken);
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = queueLimit,
                AutoReplenishment = true
            });
    });
});

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicyName);
app.UseRateLimiter();

app.MapHealthChecks("/health");
app.MapPortfolioEndpoints();

app.Run();

public partial class Program;
