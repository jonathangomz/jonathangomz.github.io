using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Portfolio.Application.Abstractions;
using Portfolio.Application.Services;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Caching;

namespace Portfolio.Tests.Services;

public sealed class PortfolioServiceTests
{
    private static (PortfolioService Service, CountingPortfolioRepository Repository, IMemoryCache MemoryCache) CreateSUT()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cacheService = new MemoryCacheService(memoryCache, NullLogger<MemoryCacheService>.Instance);
        var repository = new CountingPortfolioRepository();
        var service = new PortfolioService(repository, cacheService);
        return (service, repository, memoryCache);
    }

    [Fact]
    public async Task GetAboutAsync_PopulatesCacheOnFirstCall_AndReturnsCachedValueOnSubsequentCalls()
    {
        var (service, repository, memoryCache) = CreateSUT();

        var first = await service.GetAboutAsync();
        var second = await service.GetAboutAsync();

        Assert.Equal(1, repository.AboutCalls);
        Assert.Same(first, second);
        Assert.True(memoryCache.TryGetValue(PortfolioService.AboutCacheKey, out AboutInfo? cached));
        Assert.Same(first, cached);
    }

    [Fact]
    public async Task GetExperienceAsync_PopulatesCacheOnFirstCall_AndReturnsCachedValueOnSubsequentCalls()
    {
        var (service, repository, memoryCache) = CreateSUT();

        var first = await service.GetExperienceAsync();
        var second = await service.GetExperienceAsync();

        Assert.Equal(1, repository.ExperienceCalls);
        Assert.Same(first, second);
        Assert.True(memoryCache.TryGetValue(PortfolioService.ExperienceCacheKey, out IReadOnlyList<JobExperience>? cached));
        Assert.Same(first, cached);
    }

    [Fact]
    public async Task GetStackAsync_PopulatesCacheOnFirstCall_AndReturnsCachedValueOnSubsequentCalls()
    {
        var (service, repository, memoryCache) = CreateSUT();

        var first = await service.GetStackAsync();
        var second = await service.GetStackAsync();

        Assert.Equal(1, repository.StackCalls);
        Assert.Same(first, second);
        Assert.True(memoryCache.TryGetValue(PortfolioService.StackCacheKey, out IReadOnlyDictionary<string, IReadOnlyList<string>>? cached));
        Assert.Same(first, cached);
    }

    [Fact]
    public async Task GetProjectsAsync_PopulatesCacheOnFirstCall_AndReturnsCachedValueOnSubsequentCalls()
    {
        var (service, repository, memoryCache) = CreateSUT();

        var first = await service.GetProjectsAsync();
        var second = await service.GetProjectsAsync();

        Assert.Equal(1, repository.ProjectsCalls);
        Assert.Same(first, second);
        Assert.True(memoryCache.TryGetValue(PortfolioService.ProjectsCacheKey, out IReadOnlyList<ProjectItem>? cached));
        Assert.Same(first, cached);
    }

    [Fact]
    public async Task GetContactAsync_PopulatesCacheOnFirstCall_AndReturnsCachedValueOnSubsequentCalls()
    {
        var (service, repository, memoryCache) = CreateSUT();

        var first = await service.GetContactAsync();
        var second = await service.GetContactAsync();

        Assert.Equal(1, repository.ContactCalls);
        Assert.Same(first, second);
        Assert.True(memoryCache.TryGetValue(PortfolioService.ContactCacheKey, out IReadOnlyList<ContactLink>? cached));
        Assert.Same(first, cached);
    }

    [Fact]
    public async Task GetPortfolioAsync_AggregatesAllSections_AndReusesCachedEntries()
    {
        var (service, repository, _) = CreateSUT();

        var first = await service.GetPortfolioAsync();
        var second = await service.GetPortfolioAsync();

        Assert.NotNull(first.About);
        Assert.NotEmpty(first.Experience);
        Assert.NotEmpty(first.Stack);
        Assert.NotEmpty(first.Projects);
        Assert.NotEmpty(first.Contact);

        Assert.Equal(1, repository.AboutCalls);
        Assert.Equal(1, repository.ExperienceCalls);
        Assert.Equal(1, repository.StackCalls);
        Assert.Equal(1, repository.ProjectsCalls);
        Assert.Equal(1, repository.ContactCalls);

        Assert.Same(first.About, second.About);
        Assert.Same(first.Experience, second.Experience);
        Assert.Same(first.Stack, second.Stack);
        Assert.Same(first.Projects, second.Projects);
        Assert.Same(first.Contact, second.Contact);
    }

    private sealed class CountingPortfolioRepository : IPortfolioRepository
    {
        public int AboutCalls { get; private set; }
        public int ExperienceCalls { get; private set; }
        public int StackCalls { get; private set; }
        public int ProjectsCalls { get; private set; }
        public int ContactCalls { get; private set; }

        private static readonly AboutInfo SampleAbout = new(["Paragraph 1"], "Philosophy");
        private static readonly IReadOnlyList<JobExperience> SampleExperience =
        [
            new("One Card", "Software Developer", "Dec 2021 — present", "Monterrey", ["Highlight"])
        ];
        private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> SampleStack =
            new Dictionary<string, IReadOnlyList<string>> { ["languages"] = ["C#"] };
        private static readonly IReadOnlyList<ProjectItem> SampleProjects =
        [
            new("notion_api", "Details", "https://pub.dev/packages/notion_api", "Go to pub.dev", "https://github.com/jonathangomz/notion_api")
        ];
        private static readonly IReadOnlyList<ContactLink> SampleContact =
        [
            new("github", "github.com/jonathangomz", "https://github.com/jonathangomz", "github")
        ];

        public Task<AboutInfo> GetAboutAsync(CancellationToken ct = default)
        {
            AboutCalls++;
            return Task.FromResult(SampleAbout);
        }

        public Task<IReadOnlyList<JobExperience>> GetExperienceAsync(CancellationToken ct = default)
        {
            ExperienceCalls++;
            return Task.FromResult(SampleExperience);
        }

        public Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> GetStackAsync(CancellationToken ct = default)
        {
            StackCalls++;
            return Task.FromResult(SampleStack);
        }

        public Task<IReadOnlyList<ProjectItem>> GetProjectsAsync(CancellationToken ct = default)
        {
            ProjectsCalls++;
            return Task.FromResult(SampleProjects);
        }

        public Task<IReadOnlyList<ContactLink>> GetContactAsync(CancellationToken ct = default)
        {
            ContactCalls++;
            return Task.FromResult(SampleContact);
        }
    }
}
