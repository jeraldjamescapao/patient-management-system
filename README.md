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

### Tests

- Unit tests for `AuthService` covering all 7 flows
- `RegisterTests` — email conflict, user creation failure, role assignment failure, email delivery failure, success
- `LoginTests` — user not found, account deactivated, email not confirmed, invalid password, success
- `RefreshTests` — empty token, token not found, revoked without replacement, expired token, reuse detected (full family revocation), user not found, success
- `LogoutTests` — empty token, token not found or inactive, valid token revoked
- `LogoutAllTests` — all sessions revoked for user
- `ConfirmEmailTests` — user not found, already confirmed, invalid token, success
- `ResendConfirmationEmailTests` — user not found (silent), already confirmed (silent), email delivery failure, success
- xUnit, NSubstitute, FluentAssertions

## Tech Stack

- ASP.NET Core (.NET 10)
- PostgreSQL 17 (Docker)
- Entity Framework Core 10
- ASP.NET Core Identity
- Serilog
- MailKit

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

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker Desktop

### Run the database

```bash
docker-compose up -d
```

PostgreSQL will be available at `localhost:5432`.

### Apply migrations

From the solution root:

```bash
dotnet ef database update --project src/MedCore.Modules.Identity/MedCore.Modules.Identity.csproj
```

### Run the tests

From the solution root:

```bash
dotnet test
```

### Email (development)

The app uses Ethereal Email for development. No real emails are delivered.
Credentials in `appsettings.json` are intentional for reviewer convenience.
To use your own Ethereal account, create a free one at https://ethereal.email
and override the `Email` section in `appsettings.Development.json`.

### API docs

Start the app in Development mode and visit: https://localhost:7212/scalar/v1

### Structured logs (Seq)

Seq runs as a Docker service alongside PostgreSQL. Once the containers are up,
visit http://localhost:5341 to browse and query structured logs in real time.

Logs are also written to rolling daily JSON files under `logs/` in the project root.

### Try the API with MedCore.http

The repo includes `src/MedCore.Api/MedCore.http` with all auth endpoints
pre-configured and ready to run. It works in:

- **JetBrains Rider** — built-in HTTP client, no setup needed
- **Visual Studio** — built-in HTTP client, no setup needed
- **VS Code** — install the [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension by Huachao Mao

Open the file, set `@AccessToken` to a token from a login response, and run
any request directly from your editor.

## Status

Actively in development. Identity module is complete with unit tests in place.
Patients, Doctors, and Appointments modules coming next.

## About the Author

Jerald James Capao — Software Engineer

GitHub: https://github.com/jeraldjamescapao

This project is designed and implemented end-to-end, including architecture, domain modeling, and technical decisions.
AI tools such as Anthropic Claude are used as support during development.