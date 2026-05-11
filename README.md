# MedCore

MedCore is a healthcare API for managing patients, doctors, and appointments.
Built as a portfolio project showcasing .NET 10 modular monolith architecture
with clean layer separation, JWT authentication, refresh token rotation,
and structured logging. Built with ASP.NET Core, EF Core, and SQL Server.

## The Story

I built MedCore to show how I approach backend development. The domain is healthcare:
patients, doctors, and appointments. I wanted a codebase where every decision has
a reason I can defend, not just code that runs.

## What is built so far

**Identity** — JWT authentication with refresh token rotation, theft detection,
SHA-256 token hashing, HttpOnly cookie delivery, email confirmation via MailKit,
role-based access control, and a background service for expired token cleanup.

**Users** — profile management with culture preference per user. User ID is always
resolved from the JWT token to prevent IDOR.

**Localization** — translations stored in SQL Server, served from an in-memory cache
with a sliding expiry and a culture fallback chain (e.g. `fr-CH → fr → en`).
Admins can create, update, and soft-delete translations without a code change or restart.

## Tests

63 unit tests across three projects using xUnit, NSubstitute, and FluentAssertions.
Each service is tested in isolation with faked dependencies.

## Architecture

Modular monolith with clean layer separation: Domain, Application,
Infrastructure, and Presentation. Module boundaries are designed so
each module can be extracted into a standalone microservice.

Each module registers its own services, persistence, and controllers.
The API host only handles startup wiring.

`IIdentityUnitOfWork` is introduced in the Application layer to keep
`IdentityDbContext` out of `AuthService`. It enforces the Dependency
Inversion Principle, makes the service unit-testable without a real database,
and prepares the module for extraction without restructuring the service layer.

`IMessageLocalizer` and `ILocalizerCache` are separated into two interfaces
following the Interface Segregation Principle. Email service depends only on
`IMessageLocalizer`. Cache warmup on startup and admin refresh depend only on
`ILocalizerCache`. One implementation (`DbMessageLocalizer`) satisfies both.

The Users module accesses `ApplicationUser` via `UserManager<ApplicationUser>` and shares
the `Identity.Users` table with the Identity module. This is a known tradeoff in modular
monoliths. Identity owns credentials and tokens. Users owns profile data. On extraction,
Identity publishes a `UserRegisteredEvent` and Users maintains its own copy of profile
data in a separate database.

The data layer uses EF Core with SQL Server as the provider. Switching providers
requires updating `UseSqlServer` in each `DbContext` registration and regenerating
migrations.

## Tech Stack

- ASP.NET Core (.NET 10)
- SQL Server 2022 (Docker)
- Entity Framework Core 10
- ASP.NET Core Identity
- Serilog
- MailKit
- Seq (structured log viewer, Docker)

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- dotnet-ef CLI (`dotnet tool install --global dotnet-ef`)
- Trusted HTTPS dev certificate (`dotnet dev-certs https --trust`)

#### Apple Silicon (M1/M2/M3/M4/M5)

The official SQL Server Docker image does not support ARM64.
Use `azure-sql-edge` as a drop-in replacement. In `docker-compose.yml`, replace:

```yaml
image: mcr.microsoft.com/mssql/server:2022-latest
```

with:

```yaml
image: mcr.microsoft.com/azure-sql-edge
```

### Local configuration

The repository includes `appsettings.Example.json` with a complete
development configuration for reviewer convenience.

Copy the example file:

```bash
cp src/MedCore.Api/appsettings.Example.json src/MedCore.Api/appsettings.Development.json
```

### Run the services

```bash
docker-compose up -d
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

### Run the API

From the solution root:

```bash
dotnet run --project src/MedCore.Api/MedCore.Api.csproj --launch-profile https
```

The API will be available at `https://localhost:7212`.

On startup, the app seeds roles, the default admin account, and translations automatically,
then loads the translation cache before accepting requests.

### Run the tests

From the solution root:

```bash
dotnet test
```

### API docs

The API is versioned. All endpoints are available under `/api/v1/`.

With the API running, visit: https://localhost:7212/scalar/v1

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
Credentials in `appsettings.json` are intentional for reviewer convenience.
To use your own Ethereal account, create a free one at https://ethereal.email
and override the `Email` section in `appsettings.Development.json`.

### Default admin account

A default admin account is seeded on first startup for reviewer convenience:

- **Email**: admin@medcore.dev
- **Password**: Admin_MedCore_2026!

Use this account to log in and test the translation management endpoints.

## Status

Actively in development. Identity, Users, and Localization modules are complete with unit test coverage.
CodeItems, Patients, Doctors, and Appointments modules are next.

## About the Author

Jerald James Capao — Software Engineer

GitHub: https://github.com/jeraldjamescapao

Architecture, domain modeling, and technical decisions are all my own.
AI tools such as Anthropic Claude are used as support during development.