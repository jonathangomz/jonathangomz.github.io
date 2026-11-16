# jonathangomz.github.io

Personal site, built with [Astro](https://astro.build) and deployed to GitHub Pages.

## Commands

| Command           | Action                                       |
| :---------------- | :------------------------------------------- |
| `npm install`     | Install dependencies                          |
| `npm run dev`     | Start the dev server at `localhost:4321`      |
| `npm run build`   | Build the production site to `./dist/`        |
| `npm run preview` | Preview the build locally before deploying    |

## Project structure

```
public/            static assets served as-is (favicon)
src/
  assets/fonts/    self-hosted JetBrains Mono font
  components/      Header, Footer, Route, JsonBlock, ContactBlock, ProjectCard, SocialIcon
  data/            profile.ts (about, experience, stack, contact), projects.ts
  layouts/         BaseLayout.astro — shared <head>, fonts, theme & boot scripts
  pages/           index.astro, 404.astro
  styles/          reset.css, styles.css
```

To add a project to the home page, append an entry to `src/data/projects.ts`.

## Deployment

Pushing to `master` triggers `.github/workflows/deploy.yml`, which builds the
site and publishes `dist/` to GitHub Pages.

This requires **Settings → Pages → Build and deployment → Source** to be set to
**GitHub Actions** (previously it served the repository root directly).
