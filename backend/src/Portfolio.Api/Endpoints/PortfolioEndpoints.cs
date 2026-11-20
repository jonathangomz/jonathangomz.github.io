using Microsoft.AspNetCore.Http.HttpResults;
using Portfolio.Application.Abstractions;
using Portfolio.Domain.Entities;

namespace Portfolio.Api.Endpoints;

public static class PortfolioEndpoints
{
    public static IEndpointRouteBuilder MapPortfolioEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(string.Empty).WithTags("Portfolio");

        group.MapGet("/about", async (IPortfolioService service, CancellationToken ct) =>
        {
            var about = await service.GetAboutAsync(ct);
            return TypedResults.Ok(about);
        })
        .WithName("GetAbout");

        group.MapGet("/experience", async (IPortfolioService service, CancellationToken ct) =>
        {
            var experience = await service.GetExperienceAsync(ct);
            return TypedResults.Ok(experience);
        })
        .WithName("GetExperience");

        group.MapGet("/stack", async (IPortfolioService service, CancellationToken ct) =>
        {
            var stack = await service.GetStackAsync(ct);
            return TypedResults.Ok(stack);
        })
        .WithName("GetStack");

        group.MapGet("/projects", async (IPortfolioService service, CancellationToken ct) =>
        {
            var projects = await service.GetProjectsAsync(ct);
            return TypedResults.Ok(projects);
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

        return endpoints;
    }
}
