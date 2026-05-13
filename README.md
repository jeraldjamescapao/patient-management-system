# MedCore

MedCore is a healthcare API for managing patients, doctors, and appointments.
Built as a backend engineering portfolio project showcasing .NET 10 modular monolith architecture
with clean layer separation, JWT authentication, refresh token rotation,
and structured logging. Built with ASP.NET Core, EF Core, and SQL Server.

## The Story

I built MedCore to demonstrate how I design and structure backend systems.
The domain is healthcare: patients, doctors, and appointments.
I wanted a codebase where every decision has a reason I can defend, not just code that runs.

## Implemented Modules

**Identity** — JWT authentication with refresh token rotation, refresh token replay detection,
SHA-256 token hashing, HttpOnly cookie delivery, email confirmation via MailKit,
role-based access control, and a background cleanup service for expired refresh tokens.

**Users** — profile management with culture preference per user. User ID is always
resolved from the JWT token instead of client input to prevent IDOR.

**Localization** — translations stored in SQL Server, served from an in-memory cache
with explicit admin-triggered refresh and a culture fallback chain (e.g. `fr-CH → fr → en`).
Admins can create, update, and soft-delete translations without a code change or restart.

**CodeItems** — admin-managed healthcare reference data
(appointment types, patient classifications, doctor roles, and more).

Categories and items support activation, deactivation,
soft delete, and sort order control.

Code items and categories use the dedicated `CodeItems.Translations` table
for multilingual labels, resolved per request via `ICurrentCultureService`.

Consumer endpoints return active items with culture-resolved labels and fall back
to English when no translation exists for the requested culture.

Seed data covers Swiss clinic and hospital conventions across English,
French, and German.

## Tests

125 unit tests across four projects using xUnit, NSubstitute, and FluentAssertions.
Each service is tested in isolation with substituted infrastructure dependencies.

## Architecture

Modular monolith with clean layer separation: Domain, Application,
Infrastructure, and Presentation. Module boundaries are designed so
each module can be extracted into a standalone microservice.

Each module registers its own services, persistence, and controllers.
The API host only handles startup wiring.

For detailed design decisions, see [Architecture](docs/Architecture.md).

## Tech Stack

- ASP.NET Core (.NET 10)
- Entity Framework Core 10
- SQL Server 2022 (Docker)
- ASP.NET Core Identity
- Serilog
- Seq (structured log viewer, Docker)
- MailKit
- xUnit
- NSubstitute
- FluentAssertions

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- dotnet-ef CLI (`dotnet tool install --global dotnet-ef`)
- Trusted HTTPS dev certificate (`dotnet dev-certs https --trust`)

#### Apple Silicon (M1/M2/M3/M4/M5)

The official SQL Server Linux container image has limited ARM64 support
and may not run reliably on Apple Silicon.
Use `azure-sql-edge` as a drop-in replacement. In `docker-compose.yml`, replace:

```yaml
image: mcr.microsoft.com/mssql/server:2022-latest
```

with:

```yaml
image: mcr.microsoft.com/azure-sql-edge
```

### Local configuration

The repository includes `appsettings.Example.json`
with a sample development configuration for reviewer convenience.

Copy the example file:

```bash
cp src/MedCore.Api/appsettings.Example.json src/MedCore.Api/appsettings.Development.json
```

### Run the services

```bash
docker compose up -d
```

SQL Server will be available at `localhost:1433`.
Seq will be available at `http://localhost:5341`.

### Apply migrations

From the solution root:

Apply Identity migrations:

```bash
dotnet ef database update --project src/MedCore.Modules.Identity/MedCore.Modules.Identity.csproj --startup-project src/MedCore.Api/MedCore.Api.csproj --context IdentityDbContext
```

Apply Localization migrations:

```bash
dotnet ef database update --project src/MedCore.Modules.Localization/MedCore.Modules.Localization.csproj --startup-project src/MedCore.Api/MedCore.Api.csproj --context LocalizationDbContext
```

Apply CodeItems migrations:

```bash
dotnet ef database update --project src/MedCore.Modules.CodeItems/MedCore.Modules.CodeItems.csproj --startup-project src/MedCore.Api/MedCore.Api.csproj --context CodeItemsDbContext
```

### Run the API

From the solution root:

```bash
dotnet run --project src/MedCore.Api/MedCore.Api.csproj --launch-profile https
```

The API will be available at `https://localhost:7212`.

On startup, the app seeds roles, the default admin account, translations, and code items
automatically, then loads the translation cache before accepting requests.

### Run the tests

From the solution root:

```bash
dotnet test
```

### API docs

The API is versioned. All endpoints are available under `/api/v1/`.

With the API running, visit `https://localhost:7212/scalar/v1`.

### Structured logs (Seq)

Visit `http://localhost:5341` to browse and query structured logs in real time.

### Try the API with MedCore.http

The repo includes `src/MedCore.Api/MedCore.http` with all endpoints
pre-configured and ready to run. It works in:

- **JetBrains Rider**: built-in HTTP client, no setup needed
- **Visual Studio**: built-in HTTP client, no setup needed
- **VS Code**: install the [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension by Huachao Mao

Open the file, start the API first, then set `@AccessToken` to a token from a login or register response and run any request directly from your editor.

### Email (development)

The app uses Ethereal Email for development. No real emails are delivered.
Credentials in `appsettings.Example.json` are intentional for reviewer convenience.
To use your own Ethereal account, create a free one at `https://ethereal.email`
and override the `Email` section in `appsettings.Development.json`.

### Default admin account

A default admin account is seeded on first startup for reviewer convenience:

- **Email**: admin@medcore.dev
- **Password**: Admin_MedCore_2026!

Use this account to log in and test the admin endpoints.

## Status

The project is under active development.

Identity, Users, Localization, and CodeItems modules are implemented with unit test coverage.

Patients, Doctors, and Appointments modules are next.

## About the Author

Jerald James Capao — Software Engineer

GitHub: `https://github.com/jeraldjamescapao`

Architecture, domain modeling, and technical decisions are all my own.
AI tools such as Anthropic Claude are used for development support.