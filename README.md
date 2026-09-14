# Retro Hiscore

Friend RetroAchievements leaderboard tracker: C# API + Next.js dashboard.

## Stack

- API: ASP.NET Core, EF Core, Postgres, Hangfire, Scalar/OpenAPI
- Web: Next.js, React, Tailwind, shadcn
- Orchestration: mise + Docker Compose

## Quick start (Docker, hot reload)

1. Copy `.env.example` to `.env` and set `RA__ApiKey` (shared catalog key), `AUTH_ADMIN_DISCORD_USER_IDS`, and Discord OAuth vars.
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

### Run CI locally (Linux containers)

API integration tests use Testcontainers and behave like GitHub Actions only inside Linux with Docker socket access. Run the same CI jobs locally before pushing:

```bash
# both test-api and test-web
mise run ci:local

# or individually
mise run ci:local:api
mise run ci:local:web

# without mise
./scripts/ci-local.sh all        # Git Bash / WSL / macOS / Linux
pwsh ./scripts/ci-local.ps1 -Job all
```

Requires Docker Desktop (or Docker Engine). The API job mounts `/var/run/docker.sock` so Testcontainers can start Postgres, matching the [CI workflow](.github/workflows/ci.yml).

## Tracked games

Games are added when members link RA accounts and sync runs (recent games → track queue → admin approval), or when an admin adds a game manually. No default game list is seeded at startup.

Sync every 15 minutes + manual refresh (60s cooldown)

## Admin sync metrics

- Set `AUTH_ADMIN_DISCORD_USER_IDS` to a comma-separated list of Discord user IDs. Each person must also be invited as a member (Admin → member invites) before they can sign in.
- Admins see an **Admin** nav link and `/admin` with sync health, run history, member API key coverage, and Hangfire recurring job context (`GET /api/admin/ops`).

## Member API keys and Discord avatars

- Admins invite friends by Discord user ID in the web app (Admin → member invites).
- Each user links their RetroAchievements account and API key under **Settings**. Score sync uses that member's key; catalog/metadata uses the shared `RA__ApiKey` with pool failover across all configured keys.
- Discord avatars refresh on login and appear across the dashboard, standings, and member pages.
