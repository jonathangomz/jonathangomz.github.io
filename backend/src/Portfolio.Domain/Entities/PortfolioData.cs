namespace Portfolio.Domain.Entities;

public sealed record PortfolioData(
    AboutInfo About,
    IReadOnlyList<JobExperience> Experience,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Stack,
    IReadOnlyList<ProjectItem> Projects,
    IReadOnlyList<ContactLink> Contact);
