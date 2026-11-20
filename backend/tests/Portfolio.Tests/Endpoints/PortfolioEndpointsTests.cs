using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
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
}
