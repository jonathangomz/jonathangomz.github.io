# jonathangomz.github.io

Personal portfolio monorepo containing the [Astro](https://astro.build) static frontend (`frontend/`) and the .NET 11 Clean Architecture backend API (`backend/`).

The frontend renders static fallback data at build time so the site loads immediately even when offline, and hydrates live route data in parallel from the .NET 11 API when online — driving a real-time health status indicator in the header (flickering green dot when the API is online, solid red dot when offline).

## Frontend Commands (run inside `frontend/`)

| Command            | Action                                       |
| :----------------- | :------------------------------------------- |
| `pnpm install`     | Install dependencies                          |
| `pnpm run dev`     | Start the dev server at `localhost:4321`      |
| `pnpm run check`   | Run Astro & TypeScript diagnostics            |
| `pnpm run build`   | Type-check and build the site to `./dist/`    |
| `pnpm run preview` | Preview the build locally before deploying    |

Set `PUBLIC_API_URL` to point the frontend at a custom backend URL (defaults to `http://localhost:5080`).

## Backend Commands (run inside `backend/`)

| Command                                              | Action                                              |
| :--------------------------------------------------- | :-------------------------------------------------- |
| `dotnet build Portfolio.slnx`                        | Build the .NET 11 Clean Architecture solution       |
| `dotnet test Portfolio.slnx`                         | Run xUnit unit and integration tests                |
| `dotnet run --project src/Portfolio.Api`             | Start the Minimal API at `http://localhost:5080`    |

### API Endpoints

- `GET /health` — Health check endpoint (`200 OK` / `Healthy`)
- `GET /about` — About paragraphs and engineering philosophy (`text/markdown` by default, or `application/json` via `Accept: application/json`)
- `GET /experience` — Work experience entries (`text/markdown` by default, or `application/json` via `Accept: application/json`)
- `GET /stack` — Technology stack grouped by category (`application/json`)
- `GET /projects` — Featured open-source and production projects (`text/html` by default, or `application/json` via `Accept: application/json`)
- `GET /contact` — Social & contact links (`application/json`)
- `GET /portfolio` — Aggregated portfolio payload (`application/json`)

## Project structure

```
frontend/
  public/            static assets served as-is (favicon)
  src/
    assets/fonts/    self-hosted JetBrains Mono font
    components/      Header, Footer, Route, JsonBlock, ContactBlock, ProjectCard, SocialIcon
    data/            profile.ts (about, experience, stack, contact), projects.ts (static fallback)
    layouts/         BaseLayout.astro — shared <head>, fonts, theme & boot scripts
    pages/           index.astro, 404.astro
    scripts/         api-client.ts — /health polling, parallel route fetching & DOM hydration
    styles/          reset.css, styles.css
    utils/           json.ts — shared JSON formatting helpers

backend/
  Portfolio.slnx
  src/
    Portfolio.Domain/          Core portfolio entities (zero external dependencies)
    Portfolio.Application/     IPortfolioRepository, ICacheService, IPortfolioService, PortfolioService
    Portfolio.Infrastructure/  StaticPortfolioRepository, MemoryCacheService (IMemoryCache)
    Portfolio.Api/             Minimal API endpoints, health checks, CORS, OpenAPI
  tests/
    Portfolio.Tests/           xUnit unit and WebApplicationFactory integration tests
```

To add a project to the home page, update `frontend/src/data/projects.ts` (static fallback) and `backend/src/Portfolio.Infrastructure/Persistence/StaticPortfolioRepository.cs`.

## Deployment

Pushing to `master` (affecting `frontend/**`) triggers `.github/workflows/deploy.yml`, which builds the
site and publishes `frontend/dist/` to GitHub Pages.

This requires **Settings → Pages → Build and deployment → Source** to be set to
**GitHub Actions** (previously it served the repository root directly).
