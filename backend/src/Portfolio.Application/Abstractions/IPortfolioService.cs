using Portfolio.Domain.Entities;

namespace Portfolio.Application.Abstractions;

public interface IPortfolioService
{
    Task<AboutInfo> GetAboutAsync(CancellationToken ct = default);
    Task<IReadOnlyList<JobExperience>> GetExperienceAsync(CancellationToken ct = default);
    Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> GetStackAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProjectItem>> GetProjectsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ContactLink>> GetContactAsync(CancellationToken ct = default);
    Task<PortfolioData> GetPortfolioAsync(CancellationToken ct = default);
}
