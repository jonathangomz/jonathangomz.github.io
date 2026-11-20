using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Net.Http.Headers;
using Portfolio.Api.Logging;
using Portfolio.Application.Abstractions;
using Portfolio.Domain.Entities;

namespace Portfolio.Api.Endpoints;

public static class PortfolioEndpoints
{
    public const string LogStreamRateLimitPolicy = "LogStreamConcurrency";

    private const string MarkdownContentType = "text/markdown; charset=utf-8";
    private const string HtmlContentType = "text/html; charset=utf-8";
    private const string EventStreamContentType = "text/event-stream; charset=utf-8";
    private const string JsonMediaType = "application/json";
    private const string MarkdownMediaType = "text/markdown";
    private const string HtmlMediaType = "text/html";

    private static readonly JsonSerializerOptions SseJsonOptions = new(JsonSerializerDefaults.Web);

    private const string GitHubIconSvg =
        "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 448 512\" width=\"0.875em\" height=\"1em\" " +
        "fill=\"currentColor\" aria-hidden=\"true\" focusable=\"false\" style=\"display:inline-block;vertical-align:-0.125em\">" +
        "<path d=\"M384 32c35.3 0 64 28.7 64 64l0 320c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 96C0 60.7 28.7 32 64 32l320 0zM223.7 96c-88.4 0-159.7 72.2-159.7 160.6 0 69.4 44.1 126.9 103.4 148.4 8.4 3.1 16.6-2.5 16.6-10.9l0-25c-4.4 1.9-10 3.1-15 3.1-20.6 0-32.8-11.2-41.6-32.2-3.4-8.4-7.2-13.4-14.4-14.4-3.7-.3-5-1.9-5-3.8 0-3.7 6.2-6.6 12.5-6.6 9.1 0 16.9 5.6 25 17.2 6.2 9.1 12.8 13.1 20.6 13.1s12.8-2.8 20-10c5.3-5.3 9.4-10 13.1-13.1-41.3-5-70.3-34.7-70.3-73.1 0-15.6 5.6-32.5 15-43.8-4.1-10.3-3.4-32.2 1.2-41.2 12.5-1.6 29.4 5 39.4 14.1 11.9-3.7 24.4-5.6 39.7-5.6s27.8 1.9 39.1 5.3c9.7-8.8 26.9-15.3 39.4-13.8 4.4 8.4 5 30.3 .9 40.9 10 11.9 15.3 27.8 15.3 44.1 0 38.4-29.1 67.5-70.9 72.8 10.6 6.9 17.8 21.9 17.8 39.1l0 32.5c0 9.4 7.8 14.7 17.2 10.9 56.6-21.6 100.9-78.1 100.9-148.1 0-88.4-71.9-160.6-160.3-160.6z\" /></svg>";

    public static IEndpointRouteBuilder MapPortfolioEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(string.Empty).WithTags("Portfolio");

        group.MapGet("/about", async (HttpContext httpContext, IPortfolioService service, CancellationToken ct) =>
        {
            var about = await service.GetAboutAsync(ct);
            return PrefersJson(httpContext.Request, MarkdownMediaType)
                ? Results.Ok(about)
                : Results.Text(FormatAboutMarkdown(about), MarkdownContentType);
        })
        .WithName("GetAbout");

        group.MapGet("/experience", async (HttpContext httpContext, IPortfolioService service, CancellationToken ct) =>
        {
            var experience = await service.GetExperienceAsync(ct);
            return PrefersJson(httpContext.Request, MarkdownMediaType)
                ? Results.Ok(experience)
                : Results.Text(FormatExperienceMarkdown(experience), MarkdownContentType);
        })
        .WithName("GetExperience");

        group.MapGet("/stack", async (IPortfolioService service, CancellationToken ct) =>
        {
            var stack = await service.GetStackAsync(ct);
            return TypedResults.Ok(stack);
        })
        .WithName("GetStack");

        group.MapGet("/projects", async (HttpContext httpContext, IPortfolioService service, CancellationToken ct) =>
        {
            var projects = await service.GetProjectsAsync(ct);
            return PrefersJson(httpContext.Request, HtmlMediaType)
                ? Results.Ok(projects)
                : Results.Text(FormatProjectsHtml(projects), HtmlContentType);
        })
        .WithName("GetProjects");

        group.MapGet("/contact", async (IPortfolioService service, CancellationToken ct) =>
        {
            var contact = await service.GetContactAsync(ct);
            return TypedResults.Ok(contact);
        })
        .WithName("GetContact");

        group.MapGet("/portfolio", async (IPortfolioService service, CancellationToken ct) =>
        {
            var portfolio = await service.GetPortfolioAsync(ct);
            return TypedResults.Ok(portfolio);
        })
        .WithName("GetPortfolio");

        group.MapGet("/logs/stream", async (HttpContext httpContext, LogStreamBroadcaster broadcaster, CancellationToken ct) =>
        {
            if (!broadcaster.TryCreateSubscription(out var subscription))
            {
                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await httpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Maximum number of concurrent log stream subscribers reached. Please try again later.",
                    status = StatusCodes.Status429TooManyRequests
                }, ct);
                return;
            }

            httpContext.Response.ContentType = EventStreamContentType;
            httpContext.Response.Headers.CacheControl = "no-cache";
            httpContext.Response.Headers["X-Accel-Buffering"] = "no";

            await httpContext.Response.Body.FlushAsync(ct);

            using var lifetimeCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            lifetimeCts.CancelAfter(broadcaster.MaxConnectionDuration);

            try
            {
                await foreach (var entry in subscription.WithCancellation(lifetimeCts.Token))
                {
                    var json = JsonSerializer.Serialize(entry, SseJsonOptions);
                    await httpContext.Response.WriteAsync($"data: {json}\n\n", lifetimeCts.Token);
                    await httpContext.Response.Body.FlushAsync(lifetimeCts.Token);
                }
            }
            catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested)
            {
            }
        })
        .RequireRateLimiting(LogStreamRateLimitPolicy)
        .WithName("GetLogStream");

        return endpoints;
    }

    private static bool PrefersJson(HttpRequest request, string nativeMediaType)
    {
        var acceptHeaders = request.Headers.Accept;
        if (acceptHeaders.Count == 0)
        {
            return false;
        }

        if (!MediaTypeHeaderValue.TryParseList(acceptHeaders, out var parsed) || parsed.Count == 0)
        {
            return false;
        }

        double bestJsonQuality = -1;
        double bestNativeQuality = -1;

        foreach (var mediaType in parsed)
        {
            var quality = mediaType.Quality ?? 1.0;
            if (quality <= 0)
            {
                continue;
            }

            var value = mediaType.MediaType.Value;
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            if (value.Equals(JsonMediaType, StringComparison.OrdinalIgnoreCase) ||
                value.EndsWith("+json", StringComparison.OrdinalIgnoreCase))
            {
                if (quality > bestJsonQuality)
                {
                    bestJsonQuality = quality;
                }
            }
            else if (value.Equals(nativeMediaType, StringComparison.OrdinalIgnoreCase))
            {
                if (quality > bestNativeQuality)
                {
                    bestNativeQuality = quality;
                }
            }
        }

        return bestJsonQuality > 0 && bestJsonQuality > bestNativeQuality;
    }

    private static string FormatAboutMarkdown(AboutInfo about)
    {
        var sb = new StringBuilder();

        foreach (var paragraph in about.Paragraphs)
        {
            sb.AppendLine(paragraph);
            sb.AppendLine();
        }

        sb.Append("> ").Append(about.Philosophy);
        return sb.ToString();
    }

    private static string FormatExperienceMarkdown(IReadOnlyList<JobExperience> jobs)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < jobs.Count; i++)
        {
            if (i > 0)
            {
                sb.AppendLine();
            }

            var job = jobs[i];
            sb.Append("### ").AppendLine(job.Company);
            sb.Append(job.Role)
                .Append(" · ")
                .Append(job.Period)
                .Append(" · ")
                .AppendLine(job.Location);

            foreach (var highlight in job.Highlights)
            {
                sb.Append("- ").AppendLine(highlight);
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static string FormatProjectsHtml(IReadOnlyList<ProjectItem> projects)
    {
        var sb = new StringBuilder();
        sb.Append("<div class=\"cards\">");

        foreach (var project in projects)
        {
            var hrefLabel = project.HrefLabel ?? "Go to the page";

            sb.Append("<div class=\"card\">")
                .Append("<span class=\"card-title\">")
                .Append(WebUtility.HtmlEncode(project.Title))
                .Append("</span>")
                .Append("<hr />")
                .Append("<p class=\"card-details\">")
                .Append(WebUtility.HtmlEncode(project.Details))
                .Append("</p>")
                .Append("<div class=\"card-actions\">");

            if (!string.IsNullOrWhiteSpace(project.Href))
            {
                sb.Append("<a class=\"btn\" target=\"_blank\" href=\"")
                    .Append(WebUtility.HtmlEncode(project.Href))
                    .Append("\" rel=\"noreferrer\">")
                    .Append(WebUtility.HtmlEncode(hrefLabel))
                    .Append("</a>");
            }

            sb.Append("<a class=\"btn btn-repo\" target=\"_blank\" href=\"")
                .Append(WebUtility.HtmlEncode(project.Repo))
                .Append("\" rel=\"noreferrer\">")
                .Append(GitHubIconSvg)
                .Append("Repository</a>")
                .Append("</div>")
                .Append("</div>");
        }

        sb.Append("<div class=\"card soon\">")
            .Append("<span class=\"card-title\">Soon...</span>")
            .Append("<hr />")
            .Append("<p class=\"card-details\">Soon more projects.</p>")
            .Append("<div class=\"card-actions\"></div>")
            .Append("</div>")
            .Append("</div>");

        return sb.ToString();
    }
}
