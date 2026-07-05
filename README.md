# TrainingTracker

A full-stack workout-logging application. Users register, log workouts, review monthly
training analytics, and manage their account. Built as a technical assessment.

## Tech stack

**Backend** — ASP.NET Core Web API (.NET 10), PostgreSQL 16 via EF Core (Npgsql),
MediatR (CQRS), FluentValidation, AutoMapper, JWT bearer authentication, Scalar / OpenAPI
docs, and RFC 7807 `ProblemDetails` error handling.

**Frontend** — Angular 22 with Angular Material. Standalone components, signals, zoneless
change detection, reactive forms, and a typed HTTP layer. TypeScript strict mode; unit
tests with Vitest.

## Features

- **Authentication** — register, log in, and log out, backed by JWT. Route guards protect
  authenticated pages, and an HTTP interceptor attaches the token and signs the user out on
  `401` or deactivated-account responses.
- **Workouts** — create, list, view details, and delete training sessions (type, duration,
  calories burned, intensity, fatigue, and notes).
- **Monthly progress** — weekly breakdown with per-week and month-wide weighted averages,
  filterable by month and year.
- **Profile & account** — view profile, update details, change password, and deactivate the
  account. Security-sensitive changes re-authenticate the user, since the existing token
  still carries the old identity.
- **Health checks** — `GET /api/health` and `GET /api/health/database`.

## Prerequisites

- .NET 10 SDK
- Node.js 20+ and npm
- Docker (for PostgreSQL) — or a local PostgreSQL 16 instance
- EF Core CLI: `dotnet tool install --global dotnet-ef`

## Running locally

The default setup runs PostgreSQL in Docker while the API and frontend run on your machine.
Ports are pre-wired to match: API on `5027`, frontend on `4200`, database on `5433`.

**1. Start the database** — from `TrainingTracker/`:

```bash
docker compose up -d trainingtracker.postgres
```

This exposes PostgreSQL on `localhost:5433`, matching the connection string in
`appsettings.json`. (To use your own PostgreSQL instead, update
`ConnectionStrings:DefaultConnection`.)

**2. Apply migrations** — from `TrainingTracker/`:

```bash
dotnet ef database update
```

Migrations are **not** applied automatically at startup, so this step is required before the
first run.

**3. Run the API** — from `TrainingTracker/`:

```bash
dotnet run
```

Serves `http://localhost:5027` using the default `http` profile — which is what the frontend
expects. Interactive API docs (Scalar) are available in Development at
`http://localhost:5027/scalar/v1`, and the OpenAPI document at
`http://localhost:5027/openapi/v1.json`.

**4. Run the frontend** — from `TrainingTracker.Frontend/`:

```bash
npm install
npm start
```

Open `http://localhost:4200` and register an account to begin. There is no seed data, so the
dashboard and progress views populate as you log workouts.

> **Full Docker** `docker compose up -d` (from `TrainingTracker/`) builds and runs the
> API alongside PostgreSQL. Migrations still need to be applied once —
> run `dotnet ef database update` against the database before first use.

## Project structure

```
TrainingTracker/                 # ASP.NET Core Web API
  Features/                      # Vertical slices (Auth, Workouts, Monthly Progress, Profile, Health)
  Common/                        # Cross-cutting: pipeline behaviors, exception handling, middleware, mappings
  Database/                      # EF Core DbContext & service registration
  Migrations/                    # EF Core migrations
  docker-compose.yml             # PostgreSQL (and optional API) for local dev

TrainingTracker.Frontend/        # Angular 22 client
  src/app/
    core/                        # App-wide singletons: auth, HTTP interceptor, route guards
    features/<feature>/
      data-access/               # Typed API services & models
      pages/                     # Routed page components
      components/                # Feature-scoped components (dialogs, etc.)
    styles/                      # Design tokens & base styles
```

The backend follows a feature-folder (vertical slice) layout with a CQRS handler per use
case, a FluentValidation pipeline behavior for request validation, and centralized error
handling that returns `ProblemDetails` with a machine-readable `errorCode`. The frontend
mirrors this with a feature-based structure and a typed `data-access` service per feature.

## Configuration & security notes

This is a demo / assessment build, and a few settings favor a frictionless first run over
production hardening. They are called out here so the choices are visible rather than
accidental:

- The JWT secret and database credentials are committed in `appsettings.json` for
  convenience. In a real deployment they belong in user-secrets, environment variables, or a
  secrets manager (the project already has a `UserSecretsId` wired up for this).
- The frontend stores its JWT in `localStorage` and talks to the API over `http` on
  localhost. Angular's default output escaping is left fully intact (no `innerHTML` or
  sanitizer-bypass sinks), which keeps the token's primary exposure vector — XSS — closed.
- A single `environment.ts` targets local development. A production build would introduce a
  separate environment pointing at an HTTPS API URL.
