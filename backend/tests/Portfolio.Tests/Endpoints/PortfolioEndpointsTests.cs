using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Portfolio.Api.Logging;
using Portfolio.Domain.Entities;

namespace Portfolio.Tests.Endpoints;

public sealed class PortfolioEndpointsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetHealth_ReturnsOkAndHealthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", body);
    }

    [Fact]
    public async Task GetAbout_DefaultsToMarkdown()
    {
        var response = await _client.GetAsync("/about");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/markdown", response.Content.Headers.ContentType?.MediaType);
        var markdown = await response.Content.ReadAsStringAsync();
        Assert.Contains("Senior software developer", markdown);
        Assert.Contains(
            "> Write your code so well that you are replaceable, but write your code so well that they don't want to replace you.",
            markdown);
    }

    [Fact]
    public async Task GetAbout_WithAcceptJson_ReturnsSeededAboutInfo()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/about");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var about = await response.Content.ReadFromJsonAsync<AboutInfo>();
        Assert.NotNull(about);
        Assert.Equal(3, about.Paragraphs.Count);
        Assert.Contains("Senior software developer", about.Paragraphs[0]);
        Assert.Equal(
            "Write your code so well that you are replaceable, but write your code so well that they don't want to replace you.",
            about.Philosophy);
    }

    [Fact]
    public async Task GetExperience_DefaultsToMarkdown()
    {
        var response = await _client.GetAsync("/experience");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/markdown", response.Content.Headers.ContentType?.MediaType);
        var markdown = await response.Content.ReadAsStringAsync();
        Assert.Contains("### One Card", markdown);
        Assert.Contains("Software Developer · Dec 2021 — present · Monterrey, Nuevo León", markdown);
        Assert.Contains("### Montemorelos University", markdown);
        Assert.Contains("- Built a transaction authorizer connecting to PROSA over the ISO 8583 standard.", markdown);
    }

    [Fact]
    public async Task GetExperience_WithAcceptJson_ReturnsSeededExperienceList()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/experience");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var experience = await response.Content.ReadFromJsonAsync<List<JobExperience>>();
        Assert.NotNull(experience);
        Assert.Equal(2, experience.Count);
        Assert.Equal("One Card", experience[0].Company);
        Assert.Equal(8, experience[0].Highlights.Count);
        Assert.Equal("Montemorelos University", experience[1].Company);
        Assert.Equal(5, experience[1].Highlights.Count);
    }

    [Fact]
    public async Task GetStack_ReturnsSeededStackCategories()
    {
        var response = await _client.GetAsync("/stack");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var stack = await response.Content.ReadFromJsonAsync<Dictionary<string, List<string>>>();
        Assert.NotNull(stack);
        Assert.Equal(8, stack.Count);
        Assert.Contains("C#", stack["languages"]);
        Assert.Contains("ASP.NET Core 8+", stack["backend"]);
        Assert.Contains("ISO 8583", stack["payments"]);
    }

    [Fact]
    public async Task GetProjects_DefaultsToHtml()
    {
        var response = await _client.GetAsync("/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        var html = await response.Content.ReadAsStringAsync();
        Assert.StartsWith("<div class=\"cards\">", html);
        Assert.Contains("<span class=\"card-title\">notion_api</span>", html);
        Assert.Contains("<span class=\"card-title\">Daily quote</span>", html);
        Assert.Contains("<div class=\"card soon\">", html);
    }

    [Fact]
    public async Task GetProjects_WithAcceptJson_ReturnsSeededProjects()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/projects");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var projects = await response.Content.ReadFromJsonAsync<List<ProjectItem>>();
        Assert.NotNull(projects);
        Assert.Equal(9, projects.Count);
        Assert.Equal("notion_api", projects[0].Title);
        Assert.Equal("Daily quote", projects[^1].Title);
    }

    [Fact]
    public async Task GetContact_ReturnsSeededContacts()
    {
        var response = await _client.GetAsync("/contact");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contact = await response.Content.ReadFromJsonAsync<List<ContactLink>>();
        Assert.NotNull(contact);
        Assert.Equal(2, contact.Count);
        Assert.Equal("github", contact[0].Key);
        Assert.Equal("instagram", contact[1].Key);
    }

    [Fact]
    public async Task GetPortfolio_ReturnsCompletePortfolioData()
    {
        var response = await _client.GetAsync("/portfolio");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var portfolio = await response.Content.ReadFromJsonAsync<PortfolioData>();
        Assert.NotNull(portfolio);
        Assert.Equal(3, portfolio.About.Paragraphs.Count);
        Assert.Equal(2, portfolio.Experience.Count);
        Assert.Equal(8, portfolio.Stack.Count);
        Assert.Equal(9, portfolio.Projects.Count);
        Assert.Equal(2, portfolio.Contact.Count);
    }

    [Theory]
    [InlineData("https://jonathangomz.github.io")]
    [InlineData("http://localhost:4321")]
    public async Task AllowedOrigins_ReceiveAccessControlAllowOriginHeader(string origin)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", origin);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values));
        Assert.Contains(origin, values);
    }

    [Fact]
    public async Task DisallowedOrigin_DoesNotReceiveAccessControlAllowOriginHeader()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "https://untrusted.example.com");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task GetLogStream_ReplaysKnownRouteAndCacheLogs_AndExcludesUnknownPaths()
    {
        await _client.GetAsync("/about");
        await _client.GetAsync("/unknown-malicious-path");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var response = await _client.GetAsync(
            "/logs/stream",
            HttpCompletionOption.ResponseHeadersRead,
            cts.Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);

        await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        using var reader = new StreamReader(stream);

        var entries = new List<LogStreamEntry>();
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        while (!cts.Token.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cts.Token);
            if (line is null)
            {
                break;
            }

            if (!line.StartsWith("data: ", StringComparison.Ordinal))
            {
                continue;
            }

            var entry = JsonSerializer.Deserialize<LogStreamEntry>(line["data: ".Length..], jsonOptions);
            if (entry is not null)
            {
                entries.Add(entry);
                if (entries.Any(e => e.Source == "Request" && e.Message == "HTTP GET /about started") &&
                    entries.Any(e => e.Source == "Cache" && e.Message.Contains("portfolio:about", StringComparison.Ordinal)) &&
                    entries.Any(e => e.Source == "Request" && e.Message.StartsWith("HTTP GET /about finished with 200", StringComparison.Ordinal)))
                {
                    break;
                }
            }
        }

        Assert.NotEmpty(entries);
        Assert.Contains(entries, e => e.Source == "Request" && e.Message == "HTTP GET /about started");
        Assert.Contains(entries, e => e.Source == "Cache" && e.Message.StartsWith("Cache ", StringComparison.Ordinal) && e.Message.Contains("portfolio:about", StringComparison.Ordinal));
        Assert.Contains(entries, e => e.Source == "Request" && e.Message.StartsWith("HTTP GET /about finished with 200", StringComparison.Ordinal));
        Assert.DoesNotContain(entries, e => e.Message.Contains("/unknown-malicious-path", StringComparison.Ordinal));
        Assert.DoesNotContain(entries, e => e.Message.Contains("/logs/stream", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExceedingRateLimit_ReturnsTooManyRequestsWithRetryAfterHeader()
    {
        await using var rateLimitedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("RateLimiting:PermitLimit", "2");
            builder.UseSetting("RateLimiting:WindowSeconds", "60");
            builder.UseSetting("RateLimiting:QueueLimit", "0");
        });

        using var client = rateLimitedFactory.CreateClient();

        using var first = await client.GetAsync("/about");
        using var second = await client.GetAsync("/about");
        using var third = await client.GetAsync("/about");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
        Assert.True(third.Headers.RetryAfter?.Delta > TimeSpan.Zero);

        var body = await third.Content.ReadAsStringAsync();
        Assert.Contains("Too many requests", body);
    }

    [Fact]
    public async Task ExceedingLogStreamConcurrencyPerIp_ReturnsTooManyRequests()
    {
        await using var concurrencyFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("RateLimiting:LogStream:MaxConcurrentConnectionsPerIp", "1");
        });

        using var client = concurrencyFactory.CreateClient();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        using var firstStreamResponse = await client.GetAsync(
            "/logs/stream",
            HttpCompletionOption.ResponseHeadersRead,
            cts.Token);

        Assert.Equal(HttpStatusCode.OK, firstStreamResponse.StatusCode);

        using var secondStreamResponse = await client.GetAsync(
            "/logs/stream",
            HttpCompletionOption.ResponseHeadersRead,
            cts.Token);

        Assert.Equal(HttpStatusCode.TooManyRequests, secondStreamResponse.StatusCode);
    }

    [Fact]
    public async Task ExceedingLogStreamGlobalSubscribersCap_ReturnsTooManyRequests()
    {
        await using var globalCapFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("RateLimiting:LogStream:MaxConcurrentConnectionsPerIp", "5");
            builder.UseSetting("RateLimiting:LogStream:MaxGlobalSubscribers", "1");
        });

        using var client = globalCapFactory.CreateClient();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        using var firstStreamResponse = await client.GetAsync(
            "/logs/stream",
            HttpCompletionOption.ResponseHeadersRead,
            cts.Token);

        Assert.Equal(HttpStatusCode.OK, firstStreamResponse.StatusCode);

        using var secondStreamResponse = await client.GetAsync(
            "/logs/stream",
            HttpCompletionOption.ResponseHeadersRead,
            cts.Token);

        Assert.Equal(HttpStatusCode.TooManyRequests, secondStreamResponse.StatusCode);
        var body = await secondStreamResponse.Content.ReadAsStringAsync(cts.Token);
        Assert.Contains("Maximum number of concurrent log stream subscribers reached", body);
    }

    [Fact]
    public async Task LogStream_AutomaticallyClosesAfterMaxConnectionDuration()
    {
        await using var shortDurationFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("RateLimiting:LogStream:MaxConnectionDurationSeconds", "1");
        });

        using var client = shortDurationFactory.CreateClient();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        using var response = await client.GetAsync(
            "/logs/stream",
            HttpCompletionOption.ResponseHeadersRead,
            cts.Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync(cts.Token) is not null)
        {
        }

        Assert.False(cts.IsCancellationRequested);
    }
}
