# MedCore

MedCore is a healthcare API for managing patients, doctors, and appointments.
Built as a portfolio project showcasing .NET 10 modular monolith architecture
with clean layer separation, JWT authentication, refresh token rotation,
and structured logging. Built with ASP.NET Core, EF Core, and PostgreSQL.

## The Story

I built MedCore to demonstrate how a production-ready backend is structured,
not just that it works, but that it is built to last. The domain is healthcare:
patients, doctors, and appointments. Simple enough to stay focused, complex
enough to justify real architectural decisions. Every choice in this project,
from `IIdentityUnitOfWork` enforcing the Dependency Inversion Principle to
module boundaries designed for microservice extraction, reflects how I think
about software: with the next developer, the next requirement, and the next
scale in mind. MedCore is where I close the gap between knowing a pattern and
knowing when and why to use it.

## What is built so far

### Identity Module

- JWT authentication with refresh token rotation and theft detection
- SHA-256 refresh token hashing stored in PostgreSQL
- HttpOnly cookie-based token delivery
- Email confirmation flow via MailKit (Ethereal Email for development)
- Role-based access control (Admin, Patient, Doctor)
- Structured logging with Serilog
- RFC 7807 ProblemDetails error responses
- Background service for expired token cleanup
- API versioning (v1)
- API documentation via Scalar UI at `/scalar/v1`

### Internationalization

- DB-backed translations stored in the `localization` schema
- Supported cultures: `en`, `fr`, `de`, `fr-CH`, `de-CH`
- Culture resolution per request: authenticated users resolved from their stored
  preference, unauthenticated users resolved from the `Accept-Language` header,
  fallback to `en`
- Preferred culture cached per user with a 30-minute sliding expiry
- Confirmation emails delivered in the user's resolved language
- Translations updatable without redeployment — edit a row in the DB and call
  `POST /api/v1/admin/translations/refresh` to reload the cache immediately
- `PUT /api/v1/auth/culture` — authenticated users can set their preferred language
- `POST /api/v1/admin/translations/refresh` — Admin only, reloads translation cache

### Tests

- 33 unit tests for `AuthService` covering all 8 flows
- `RegisterTests` — email conflict, user creation failure, role assignment failure, email delivery failure, success
- `LoginTests` — user not found, account deactivated, email not confirmed, invalid password, success
- `RefreshTests` — empty token, token not found, revoked without replacement, expired token, reuse detected (full family revocation), user not found, success
- `LogoutTests` — empty token, token not found or inactive, valid token revoked
- `LogoutAllTests` — all sessions revoked for user
- `ConfirmEmailTests` — user not found, already confirmed, invalid token, success
- `ResendConfirmationEmailTests` — user not found (silent), already confirmed (silent), email delivery failure, success
- `UpdatePreferredCultureTests` — unsupported culture, user not found, valid base culture, valid regional culture
- xUnit, NSubstitute, FluentAssertions

## Tech Stack

- ASP.NET Core (.NET 10)
- PostgreSQL 17 (Docker)
- Entity Framework Core 10
- ASP.NET Core Identity
- Serilog
- MailKit
- Seq (structured log viewer, Docker)

## Architecture

Modular monolith with clean layer separation: Domain, Application,
Infrastructure, and Presentation. Module boundaries are designed so
each module can be extracted into a standalone microservice.

Each module registers its own services, persistence, and controllers.
The API host only handles startup wiring.

`IIdentityUnitOfWork` is introduced in the Application layer to keep
`IdentityDbContext` out of `AuthService`. This is intentional. It
enforces the Dependency Inversion Principle and makes the service
testable without a real database, even though it is overkill at this
scale. It signals the module is ready for microservice extraction.

`IMessageLocalizer` and `ILocalizerCache` are separated into two interfaces
following the Interface Segregation Principle. Email service depends only on
`IMessageLocalizer`. Startup warmup and admin refresh depend only on
`ILocalizerCache`. One implementation (`DbMessageLocalizer`) satisfies both.

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- dotnet-ef CLI (`dotnet tool install --global dotnet-ef`)
- Trusted HTTPS dev certificate (`dotnet dev-certs https --trust`)

### Run the services

```bash
docker-compose up -d
```

PostgreSQL will be available at `localhost:5432`.  
Seq will be available at `http://localhost:5341`.

### Apply migrations

From the solution root:

Apply Identity migrations:

```bash
dotnet ef database update --project src/MedCore.Modules.Identity/MedCore.Modules.Identity.csproj --startup-project src/MedCore.Api/MedCore.Api.csproj --context IdentityDbContext
```

Apply Localization migrations:

```bash
dotnet ef database update --project src/MedCore.Infrastructure/MedCore.Infrastructure.csproj --startup-project src/MedCore.Api/MedCore.Api.csproj --context LocalizationDbContext
```

### Run the API

From the solution root:

```bash
dotnet run --project src/MedCore.Api/MedCore.Api.csproj --launch-profile https
```

The API will be available at `https://localhost:7212`.

On startup, the app seeds roles and translations automatically, then warms up
the translation cache before accepting requests.

### Run the tests

From the solution root:

```bash
dotnet test
```

Runs 33 unit tests across all 8 `AuthService` flows.

### API docs

With the API running, visit: https://localhost:7212/scalar/v1

### Structured logs (Seq)

Visit `http://localhost:5341` to browse and query structured logs in real time.

Logs are also written to rolling daily JSON files under `logs/` in the project root.

### Try the API with MedCore.http

The repo includes `src/MedCore.Api/MedCore.http` with all endpoints
pre-configured and ready to run. It works in:

- **JetBrains Rider** — built-in HTTP client, no setup needed
- **Visual Studio** — built-in HTTP client, no setup needed
- **VS Code** — install the [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension by Huachao Mao

Open the file, start the API first, then set `@AccessToken` to a token from a login or register response and run any request directly from your editor.

### Email (development)

The app uses Ethereal Email for development. No real emails are delivered.
Credentials in `appsettings.json` are intentional for reviewer convenience.
To use your own Ethereal account, create a free one at https://ethereal.email
and override the `Email` section in `appsettings.Development.json`.

### Updating a translation without redeployment

1. Connect to the database and update the relevant row in `localization.translations`
2. Call `POST /api/v1/admin/translations/refresh` with an Admin Bearer token
3. The cache is cleared and reloaded immediately — no restart required

## Status

Actively in development. Identity module and internationalization foundation
are complete with unit tests in place. Patients, Doctors, and Appointments
modules coming next.

## About the Author

Jerald James Capao — Software Engineer

GitHub: https://github.com/jeraldjamescapao

This project is designed and implemented end-to-end, including architecture, domain modeling, and technical decisions.
AI tools such as Anthropic Claude are used as support during development.