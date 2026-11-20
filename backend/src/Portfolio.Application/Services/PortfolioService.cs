using Portfolio.Application.Abstractions;
using Portfolio.Domain.Entities;

namespace Portfolio.Application.Services;

public sealed class PortfolioService(
    IPortfolioRepository repository,
    ICacheService cache) : IPortfolioService
{
    public const string AboutCacheKey = "portfolio:about";
    public const string ExperienceCacheKey = "portfolio:experience";
    public const string StackCacheKey = "portfolio:stack";
    public const string ProjectsCacheKey = "portfolio:projects";
    public const string ContactCacheKey = "portfolio:contact";

    private static readonly TimeSpan DefaultCacheExpiration = TimeSpan.FromMinutes(30);

    public Task<AboutInfo> GetAboutAsync(CancellationToken ct = default) =>
        cache.GetOrCreateAsync(AboutCacheKey, repository.GetAboutAsync, DefaultCacheExpiration, ct);

    public Task<IReadOnlyList<JobExperience>> GetExperienceAsync(CancellationToken ct = default) =>
        cache.GetOrCreateAsync(ExperienceCacheKey, repository.GetExperienceAsync, DefaultCacheExpiration, ct);

    public Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> GetStackAsync(CancellationToken ct = default) =>
        cache.GetOrCreateAsync(StackCacheKey, repository.GetStackAsync, DefaultCacheExpiration, ct);

    public Task<IReadOnlyList<ProjectItem>> GetProjectsAsync(CancellationToken ct = default) =>
        cache.GetOrCreateAsync(ProjectsCacheKey, repository.GetProjectsAsync, DefaultCacheExpiration, ct);

    public Task<IReadOnlyList<ContactLink>> GetContactAsync(CancellationToken ct = default) =>
        cache.GetOrCreateAsync(ContactCacheKey, repository.GetContactAsync, DefaultCacheExpiration, ct);

    public async Task<PortfolioData> GetPortfolioAsync(CancellationToken ct = default)
    {
        var aboutTask = GetAboutAsync(ct);
        var experienceTask = GetExperienceAsync(ct);
        var stackTask = GetStackAsync(ct);
        var projectsTask = GetProjectsAsync(ct);
        var contactTask = GetContactAsync(ct);

        await Task.WhenAll(aboutTask, experienceTask, stackTask, projectsTask, contactTask);

        return new PortfolioData(
            await aboutTask,
            await experienceTask,
            await stackTask,
            await projectsTask,
            await contactTask);
    }
}
