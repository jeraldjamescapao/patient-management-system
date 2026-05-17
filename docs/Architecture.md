# Architecture

## Overview

MedCorVis is a modular monolith. Each module has its own Domain, Application,
Infrastructure, and Presentation layers. Module boundaries are designed so that
each module can be extracted independently without restructuring
its internal layers.

The API host (`MedCorVis.Api`) is responsible for startup wiring only.
It does not own any business logic or domain models.

## Project Structure

```
src/
  MedCorVis.Api                   # Host - middleware, startup, wiring only
  MedCorVis.Common                # Shared contracts, interfaces, and result types
  MedCorVis.Infrastructure        # Shared infrastructure (email via MailKit)
  MedCorVis.Modules.Identity      # Auth, JWT, refresh tokens, email confirmation
  MedCorVis.Modules.Users         # User profile, culture preference, phone
  MedCorVis.Modules.Localization  # DB-backed translations, in-memory cache
  MedCorVis.Modules.CodeItems     # Healthcare reference data, multilingual labels

tests/
  MedCorVis.Modules.Identity.Tests
  MedCorVis.Modules.Users.Tests
  MedCorVis.Modules.Localization.Tests
  MedCorVis.Modules.CodeItems.Tests
```

## Module System

Each module implements `IModule` from `MedCorVis.Common`:

```csharp
public interface IModule
{
    IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration);
    WebApplication MapEndpoints(WebApplication app);
    Task RunStartupTasksAsync(WebApplication app) => Task.CompletedTask;
}
```

`Program.cs` calls `RegisterModules(...)` once, passing each module assembly.
The module system auto-discovers all `IModule` implementations, registers their
services, and adds their controllers as application parts. `Program.cs` does not
change when a new module is added.

`RunStartupTasksAsync` is where each module runs its own seeders.

Identity seeds roles and the admin account.
Localization seeds translations and warms the in-memory cache.
CodeItems seeds the healthcare vocabulary.

## Request Pipeline

```
UseHttpsRedirection
UseSerilogRequestLogging  
  ← must be before exception handling to capture full timing
UseMiddleware<ExceptionHandlingMiddleware>
UseAuthentication
UseMiddleware<CultureMiddleware>
  ← between auth and authorization
  ← resolves culture from JWT claim or Accept-Language header
UseAuthorization
```

`CultureMiddleware` resolves the request culture in this order:

1. If the caller is authenticated, read the user's preferred culture from the cache.
2. Otherwise, parse the `Accept-Language` header.
3. Fall back to English if no supported culture is found.

## Layer Conventions

Each module follows these layer rules:

- **Domain** — entities, value objects, domain exceptions. No framework dependencies.
- **Application** — services, interfaces, request/response contracts, error codes. No EF or infrastructure types.
- **Infrastructure** — EF DbContext, repositories, migrations, seeders. Implements application interfaces.
- **Presentation** — controllers. Calls application services only. No direct infrastructure access.

`*ServiceCollectionExtensions` classes are declared as `internal static`
to keep registration logic scoped to the module.

`InfrastructureServiceCollectionExtensions` is `public static`
because it is called by the host.

`*WebApplicationExtensions` classes are `public static`
because they are called by the host.

## Persistence

The application uses Entity Framework Core with SQL Server as the database provider.

Each module owns its own `DbContext`, migrations, and persistence configuration.

Database schema separation is handled per module using dedicated schemas
(e.g. `Identity`, `Localization`, `CodeItems`).

EF Core handles all database access.

## Error Handling

Services return `Result<T>` instead of throwing exceptions. `Result<T>` carries either
a value or a typed error. `BaseApiController.ToActionResult` maps the result to the
correct HTTP status code and an RFC 7807 ProblemDetails response body.

`ExceptionHandlingMiddleware` catches anything that escapes the service layer:

- `DomainException` maps to 422 with a machine-readable `code` field.
- Any other unhandled exception maps to 500.

All error responses include `traceId` and `code` extensions:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Translation not found.",
  "instance": "/api/v1/translations/99",
  "traceId": "...",
  "code": "LOCALIZATION_TRANSLATION_NOT_FOUND"
}
```
Error code format: `MODULE_RESOURCE_DESCRIPTION` (e.g. `CODEITEMS_ITEM_NOT_FOUND`).


Model validation errors (e.g. missing required fields, invalid date ranges) return 400
with a per-field breakdown:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/categories/1/items",
  "traceId": "...",
  "errors": {
    "ValidFrom": ["ValidFrom must be before ValidTo."],
    "ValidTo":   ["ValidFrom must be before ValidTo."]
  }
}
```

## Cross-Module Communication

Modules do not reference each other's assemblies. Cross-module references use `Guid`
only — no EF navigation properties across module boundaries.

`ICurrentUserService` (in `MedCorVis.Common`) is injected wherever business logic needs
the caller's identity. User ID is always resolved from the validated JWT token.
It is never accepted from a request body or URL parameter (IDOR prevention).
Nested routes enforce parent ownership at the service layer — a child resource is
only accessible when its stored parent ID matches the route (e.g. item.CategoryId
must equal the categoryId in the URL). This applies to all modules.

## Identity Module

`IIdentityUnitOfWork` is introduced in the Application layer to keep `IdentityDbContext`
out of `AuthService`. The service depends on the interface, not the EF context directly.
This keeps the service testable and the module portable.

Refresh token rotation uses SHA-256 hashing. The raw token is sent to the client;
only the hash is stored. SHA-256 is appropriate here because refresh tokens are
already cryptographically random (high-entropy), so bcrypt is unnecessary.

Token theft detection: if a revoked token is replayed and `ReplacedByTokenId` is set,
the system treats it as a stolen token and revokes the entire token family for the user.

## Localization Module

`IMessageLocalizer` and `ILocalizerCache` are two separate interfaces. The email service
depends only on `IMessageLocalizer`. Cache warmup and admin refresh depend only on
`ILocalizerCache`. One implementation (`DbMessageLocalizer`) satisfies both.

Translations are stored in SQL Server (`Localization` schema) and loaded into an
in-memory cache on startup. The cache has no automatic expiry — it persists until
an admin triggers a reload via the cache refresh endpoint, or the API restarts.

The culture fallback chain is: `fr-CH → fr → en`.

## CodeItems Module

CodeItems is the application-wide healthcare controlled vocabulary. It is the single
source of truth for all healthcare classification terms used across modules
(appointment types, patient classifications, doctor roles, and more).

The module maintains its own translation table (`CodeItems.Translations`) separate from
`Localization.Translations`. The distinction is intentional:

- General translations (`Localization`) are stable, cache-friendly, and tolerate
  short staleness. They are loaded into an in-memory cache.
- Code item labels (`CodeItems`) are tied directly to domain data and must reflect
  admin changes immediately. They are never cached.

The module exposes two controllers:

- `CodeItemsController` — admin CRUD for categories, items, and translations.
- `CodeItemsConsumerController` — read-only consumer endpoint. Returns active items
  for a given category code with culture-resolved labels. Falls back to English when
  no label exists for the requested culture, then falls back to the item code itself.

All item operations on nested routes (`/categories/{categoryId}/items/{id}`)
verify that the item's `CategoryId` matches the route parameter before proceeding.
A mismatch returns `404 Not Found` to avoid confirming the resource exists elsewhere.

Items and categories carry `IsSystemDefined`, `IsEditable`, and `IsDeletable` flags.
System-defined records are seeded and protected from accidental deletion. Admins can
create additional entries freely.

Items support an optional validity window via `ValidFrom` and `ValidTo` (`DateOnly?`).
The consumer endpoint filters out items outside their validity window at query time.
Items with no window set are always visible. Validity is admin-controlled and mutable.

`ValidDateRangeAttribute` in `MedCorVis.Common.Validations` handles cross-field date
range validation. Any request contract with a date range can use it.

## Users Module

The Users module shares the `Identity.Users` table with the Identity module via
`UserManager<ApplicationUser>`. This is a known tradeoff in a modular monolith.
Identity owns credentials and tokens.
The Users module owns profile data.

On future extraction to microservices, Identity would publish a `UserRegisteredEvent`
and Users would maintain its own copy of profile data in a separate database.

## API Versioning

API versioning is declared at the controller level (`[Route("api/v{version:apiVersion}/...")]`),
not globally in `Program.cs`. This keeps each module self-contained and portable
on extraction.

All routes are resource-based. They describe the resource, never the role or action
(e.g. `/translations`, not `/admin/translations`).

## Logging

Structured logging uses Serilog with a Seq sink. All log messages use the
`LoggerMessage.Define` pattern — messages are compiled at startup, not on each call.

Log event ID ranges by module:

| Range     | Owner                                 |
|-----------|---------------------------------------|
| 1000s     | Api (middleware)                      |
| 2000s     | Identity / AuthService                |
| 3000s     | Users                                 |
| 4000s     | Localization                          |
| 5001-5008 | Seeders (RoleSeeder, AdminUserSeeder) |
| 6000s     | CodeItems                             |
| 7000s     | Next available                        |

## Testing

Each service is tested in isolation. Repositories, `UserManager`, and other
infrastructure dependencies are substituted using NSubstitute.

Domain logic is tested directly against entity methods with no infrastructure involved.

`InternalsVisibleTo` is declared in each module's `.csproj`
for both the test project and `DynamicProxyGenAssembly2`.

Test projects mirror the source structure: one test class file per method group,
one base class per service that wires up the SUT and shared helpers.

## Author

Jerald James Capao — Software Engineer