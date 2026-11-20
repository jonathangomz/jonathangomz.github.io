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
- `GET /logs/stream` — Real-time Server-Sent Events (`text/event-stream`) Serilog stream for known routes and cache hits/misses

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

### Frontend (GitHub Pages)

Pushing to `master` (affecting `frontend/**`) triggers `.github/workflows/deploy.yml`, which builds the
site with `PUBLIC_API_URL: ${{ vars.PUBLIC_API_URL }}` and publishes `frontend/dist/` to GitHub Pages.

This requires **Settings → Pages → Build and deployment → Source** to be set to
**GitHub Actions** (previously it served the repository root directly).

### Backend (Azure App Service)

Pushing to `master` (affecting `backend/**`) triggers `.github/workflows/deploy-backend.yml`, which restores, builds, runs `dotnet test`, publishes `Portfolio.Api`, and deploys the artifact to Azure App Service via `azure/webapps-deploy@v3`.

#### Step-by-Step Azure & GitHub Setup

1. **Create the Azure Web App (Azure Portal)**:
   - Go to **Azure Portal → Create a resource → Web App**.
   - **Name**: e.g., `jonathangomz-portfolio-api` (your default URL will be `https://jonathangomz-portfolio-api.azurewebsites.net`).
   - **Publish**: `Code`
   - **Runtime stack**: `.NET 10 (STS)` (or `.NET 11` / self-contained depending on your selected stack)
   - **Operating System**: `Linux`
   - **Pricing Plan**: `Free F1` (or `Basic B1` for Always On).
2. **Enable Basic Auth & Download Publish Profile (Azure Portal)**:
   - In your Web App, open **Settings → Configuration → General settings** and set **SCM Basic Auth Publishing Credentials** to **On**.
   - On the Web App **Overview** blade, click **Download publish profile** to download the `.PublishSettings` XML file.
3. **Configure GitHub Secrets & Variables**:
   - In your GitHub repository, go to **Settings → Secrets and variables → Actions**:
   - Under **Secrets → New repository secret**:
     - `AZURE_WEBAPP_PUBLISH_PROFILE`: Paste the full XML content of the downloaded `.PublishSettings` file.
   - Under **Variables → New repository variable**:
     - `AZURE_WEBAPP_NAME`: Your Azure Web App name (e.g., `jonathangomz-portfolio-api`).
     - `PUBLIC_API_URL`: Your Azure Web App HTTPS URL (e.g., `https://jonathangomz-portfolio-api.azurewebsites.net`).
