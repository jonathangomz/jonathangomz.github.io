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

app.MapHealthChecks("/health");
app.MapPortfolioEndpoints();

app.Run();

public partial class Program;
