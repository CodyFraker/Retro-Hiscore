# Retro Hiscore

Friend RetroAchievements leaderboard tracker: C# API + Next.js dashboard.

## Stack

- API: ASP.NET Core, EF Core, Postgres, Hangfire, Scalar/OpenAPI
- Web: Next.js, React, Tailwind, shadcn
- Orchestration: mise + Docker Compose

## Quick start (Docker, hot reload)

1. Copy `.env.example` to `.env` and set `RA__Username` / `RA__ApiKey`.
2. `docker compose --profile dev up --build` (or `mise run docker:up`)
3. Open http://localhost:18321 (frontend) and http://localhost:18943 (API / Scalar at `/scalar`).

The `dev` profile mounts source into the containers so API (`dotnet watch`) and web (`next dev`) reload on save. Rebuild after dependency changes (`*.csproj` / `package-lock.json`).

Production-like images (no bind mounts): `mise run docker:up:prod` or `docker compose --profile prod up --build`.

## Local development (host)

```bash
# Postgres via compose
docker compose up postgres -d

# API (http://localhost:18943)
dotnet run --project apps/api --urls http://localhost:18943

# Web (http://localhost:18321)
npm --prefix apps/web run dev -- --port 18321
```

Or with mise: `mise run api` / `mise run web`.

## Generate API client

```bash
mise run generate-client
# or: API_URL=http://localhost:18943 node scripts/generate-api-client.mjs
```

Uses a running API OpenAPI doc when available; otherwise the checked-in `packages/api-client/openapi.json`.

## Tests

```bash
dotnet test apps/api.tests/RetroHiscore.Api.Tests.csproj
npm --prefix apps/web test
npm --prefix apps/web run cypress:run   # web app must be running with intercepts or mocks
```

## Tracked v1 data

- Games: `38130`, `2291`, `789`
- Members: `ShrimpPoboy`, `beefboybilly`, `xXScubXx`
- Sync every 15 minutes + manual refresh (60s cooldown)
