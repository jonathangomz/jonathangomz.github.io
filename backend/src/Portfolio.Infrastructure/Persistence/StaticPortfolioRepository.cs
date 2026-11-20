using Portfolio.Application.Abstractions;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence;

public sealed class StaticPortfolioRepository : IPortfolioRepository
{
    private static readonly AboutInfo About = new(
        Paragraphs:
        [
            "Senior software developer working on production systems since 2016, specialising in backend. Strong background in designing RESTful and gRPC APIs, worker services and database architecture, refactoring legacy systems into modern, maintainable ones.",
            "Experienced in academic and financial environments, collaborating with multidisciplinary teams and delivering scalable backend services.",
            "Not a frontend specialist, but has delivered enough of it to lead those decisions, and to let the way a client actually consumes an API shape how that API is designed."
        ],
        Philosophy: "Write your code so well that you are replaceable, but write your code so well that they don't want to replace you.");

    private static readonly IReadOnlyList<JobExperience> Experience =
    [
        new(
            Company: "One Card",
            Role: "Software Developer",
            Period: "Dec 2021 — present",
            Location: "Monterrey, Nuevo León",
            Highlights:
            [
                "Built a transaction authorizer connecting to PROSA over the ISO 8583 standard.",
                "Built a TUI to orchestrate multiple authorizer instances across ports over gRPC, keeping latency low between services.",
                "Published a Clean Architecture project template to Azure Artifacts, standardising rate limiting, unit tests, OpenTelemetry and health checks across company projects.",
                "Published NuGet packages for shared logging, notifications, database connections, encryption, storage and error handling.",
                "Built an API gateway with Ocelot, improving analytics and opening the path to microservices.",
                "Led the refactor of a Single Sign-On API to centralise authentication logic.",
                "Led the migration of the API documentation system from .NET Framework 4.7.2 to .NET 10, adding OpenAPI support.",
                "Held overall test coverage at 70% with FluentAssertions, enforced by Azure Pipelines branch policies."
            ]),
        new(
            Company: "Montemorelos University",
            Role: "Full Stack Developer & Teacher",
            Period: "Dec 2016 — Dec 2021",
            Location: "Montemorelos, Nuevo León",
            Highlights:
            [
                "Led the university's open-source mobile app: Angular, later migrated to Flutter, with a NestJS API.",
                "Published open-source Dart packages to pub.dev (ummobile_sdk, ummobile_custom_http) to decouple business logic.",
                "Sole maintainer of EduAdvent, first as an undergraduate and later as staff: the J2EE codebase, PostgreSQL database and Tomcat production deploys of a school management system running in Mexico, El Salvador and Panama.",
                "Configured Linux servers with Nginx for sandbox and production environments.",
                "Taught web and software development at the Faculty of Engineering and Technology."
            ])
    ];

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Stack =
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["languages"] = ["C#", "T-SQL", "PL/pgSQL", "TypeScript", "JavaScript", "Dart", "Java"],
            ["backend"] = ["ASP.NET Core 8+", ".NET Framework 4.7.2", "NestJS", "Express", "Spring Boot"],
            ["frontend"] = ["Flutter", "Angular", "React", "Vue"],
            ["databases"] = ["SQL Server", "PostgreSQL", "MongoDB"],
            ["apis"] = ["REST", "gRPC", "OpenAPI"],
            ["auth"] = ["JWT", "OAuth 2.0", "SSO"],
            ["payments"] = ["ISO 8583"],
            ["ops"] = ["Azure DevOps", "GitHub Actions", "Nginx", "IIS"]
        };

    private static readonly IReadOnlyList<ProjectItem> Projects =
    [
        new(
            Title: "notion_api",
            Details: "A Dart client wrapping the Notion public API, published to pub.dev across 14 releases.",
            Href: "https://pub.dev/packages/notion_api",
            HrefLabel: "Go to pub.dev",
            Repo: "https://github.com/jonathangomz/notion_api"),
        new(
            Title: "UMMobile API",
            Details: "The NestJS API behind the app, collecting every other API the university exposes behind one surface.",
            Href: null,
            HrefLabel: null,
            Repo: "https://github.com/UMMobile/ummobile-api"),
        new(
            Title: "UMMobile app",
            Details: "Montemorelos University's open source mobile app, built with Flutter after migrating it from Angular.",
            Href: null,
            HrefLabel: null,
            Repo: "https://github.com/UMMobile/ummobile_app"),
        new(
            Title: "ummobile_sdk",
            Details: "A published Dart package wrapping the UMMobile API, so the app does not carry the business logic.",
            Href: "https://pub.dev/packages/ummobile_sdk",
            HrefLabel: "Go to pub.dev",
            Repo: "https://github.com/UMMobile/ummobile_sdk"),
        new(
            Title: "ummobile_custom_http",
            Details: "A published Dart package with the HTTP client the rest of the UMMobile packages are built on.",
            Href: "https://pub.dev/packages/ummobile_custom_http",
            HrefLabel: "Go to pub.dev",
            Repo: "https://github.com/UMMobile/ummobile_custom_http"),
        new(
            Title: "pnsys",
            Details: "An Express API for multi-app push notifications over OneSignal, with JWT and master key auth, roles and analytics. Reached production behind the university API gateway.",
            Href: null,
            HrefLabel: null,
            Repo: "https://github.com/jonathangomz/pnsys"),
        new(
            Title: "Devotionals API",
            Details: "A NestJS REST API serving devotional books from MongoDB, with endpoints for a whole book or just the current day.",
            Href: null,
            HrefLabel: null,
            Repo: "https://github.com/jonathangomz/devotionals-api"),
        new(
            Title: "Playlist cloner",
            Details: "Implements the Spotify OAuth authorization code flow from scratch, then clones a playlist into the signed-in account.",
            Href: "https://playlist-cloner.vercel.app/cloner",
            HrefLabel: null,
            Repo: "https://github.com/jonathangomz/playlist-cloner"),
        new(
            Title: "Daily quote",
            Details: "A simple page that displays a different quote every day.",
            Href: "https://jonathangomz.github.io/quotes/",
            HrefLabel: null,
            Repo: "https://github.com/jonathangomz/quotes")
    ];

    private static readonly IReadOnlyList<ContactLink> Contact =
    [
        new(
            Key: "github",
            Display: "github.com/jonathangomz",
            Href: "https://github.com/jonathangomz",
            Icon: "github"),
        new(
            Key: "instagram",
            Display: "instagram.com/jonathan_gomz",
            Href: "https://instagram.com/jonathan_gomz",
            Icon: "instagram")
    ];

    public Task<AboutInfo> GetAboutAsync(CancellationToken ct = default) =>
        Task.FromResult(About);

    public Task<IReadOnlyList<JobExperience>> GetExperienceAsync(CancellationToken ct = default) =>
        Task.FromResult(Experience);

    public Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> GetStackAsync(CancellationToken ct = default) =>
        Task.FromResult(Stack);

    public Task<IReadOnlyList<ProjectItem>> GetProjectsAsync(CancellationToken ct = default) =>
        Task.FromResult(Projects);

    public Task<IReadOnlyList<ContactLink>> GetContactAsync(CancellationToken ct = default) =>
        Task.FromResult(Contact);
}
