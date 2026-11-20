using Portfolio.Api.Endpoints;
using Portfolio.Application;
using Portfolio.Infrastructure;

const string CorsPolicyName = "PortfolioCors";

var builder = WebApplication.CreateBuilder(args);

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
