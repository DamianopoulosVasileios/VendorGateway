# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

VendorGateway is a Clean Architecture ASP.NET Core (.NET 10) API that acts as a gateway between clients and one or more third-party vendor systems (currently FakeStore, `https://fakestoreapi.com`). It exposes a REST API for Accounts, Orders, and Products, syncs data with the vendor, and persists its own local copy via EF Core/SQLite.

See `README.md` and `FlowChart.txt` at the repo root for the full architecture writeup — both are kept up to date and are the best starting point before making structural changes.

## Commands

- Build: `dotnet build VendorGateway.slnx` (solution uses the newer `.slnx` XML format, not a classic `.sln`)
- Run API: `dotnet run --project VendorGateway.API` (Swagger UI at `swagger/index.html`, dev-only)
- Run all tests: `dotnet test VendorGateway.Tests\VendorGateway.Tests.csproj`
- Run a single test: `dotnet test VendorGateway.Tests\VendorGateway.Tests.csproj --filter "FullyQualifiedName~CreateAsync_VendorReturnsValidId_PersistsAccountLocally"`

There is no CI pipeline (`.github/workflows` doesn't exist), no Dockerfile, and no `.editorconfig`/`Directory.Build.props`.

EF Core migrations apply automatically at startup (`db.Database.Migrate()` in `Program.cs`) — no manual `dotnet ef database update` needed. The SQLite file lives at `Infrastructure/DbFile/VendorGateway.db` relative to the working directory.

`Program.cs` determines the ASP.NET Core environment from `AppSettings:Mode` in `appsettings.json` (read via a temporary `ConfigurationBuilder` before `WebApplicationOptions` is built), not purely from `ASPNETCORE_ENVIRONMENT`.

## Architecture

Four projects, dependencies point inward only (`API → Application`, `API → Infrastructure` for DI wiring, `Infrastructure → Application`; `Application` depends on nothing else in the solution):

- **`VendorGateway.API`** — Controllers, `Api*Request`/`Api*Response` contracts, filters, API-layer mappers, `Program.cs` composition root.
- **`VendorGateway.Application`** — Use-case services, domain entities, DTOs, interfaces (`Interfaces/Services`, `Interfaces/CommandsQueries`, `Interfaces/ApiClient`), background job infrastructure. No EF Core/HTTP/vendor-specific dependencies — it only knows interfaces it defines itself.
- **`VendorGateway.Infrastructure`** — EF Core (`AppDbContext`, `Migrations`), repository Commands/Queries implementations, vendor HTTP clients (`Apis/`), exception classifiers, Infrastructure-layer mappers.
- **`VendorGateway.Tests`** — xUnit, references `VendorGateway.API` (pulls in the other two layers transitively).

### Request flow

Controller (bound to `Api*Request`) → maps to Application DTO → use-case service (`Application/Services/{Domain}/{Verb}{Domain}Service.cs`, one class per use case, e.g. `ICreateAccountService`, `IUpdateOrderService`) → business rules via Commands/Queries interfaces → Infrastructure implements via EF Core (`Repositories/{Domain}/{Domain}Commands.cs` / `{Domain}Queries.cs`) and/or vendor HTTP clients (`Apis/FakeStore{Domain}ApiClient.cs`) → result or domain exception (`KeyNotFoundException`, `InvalidOperationException`, etc.) bubbles back → controller maps to `Api*Response` + status code.

Each use case has its own service+interface rather than one fat per-domain service — keep this pattern when adding new actions.

### Asynchronous job processing

Write actions (create/update/delete/execute) don't call the vendor or persist synchronously in the request. The controller inserts a `Job` row and returns `202 Accepted` immediately; `JobProcessingBackgroundService` polls the `Jobs` table, claims batches, and dispatches to the matching use-case service by `Job.Type`. `FailedJobProcessingBackgroundService` handles retry/cleanup of failed jobs. `SyncProductsFromVendorBackgroundService` runs the product catalog sync independently of the job queue. There is no client-facing status/polling endpoint yet — job outcomes live in `Job.Status`/`Job.ErrorMessage`.

### Persistence

- Single `AppDbContext` (`Infrastructure/Persistence`), SQLite provider. `DbSet`s: `Products`, `Accounts`, `Orders`, `OrderItems`, `Jobs`, `Users`.
- Entities implementing `IAuditable` get `CreatedAt`/`UpdatedAt` auto-populated by a `SaveChangesAsync` override — never set these manually.
- Order creation idempotency is enforced by a unique constraint on `Order.IdempotencyKey`, supplied via the required `Idempotency-Key` header (`API/Filters/RequireIdempotencyKeyAttribute.cs`, returns 400 if missing/invalid).
- DB-specific failures (e.g. unique constraint violations) are abstracted behind `IDbExceptionClassifier` so repository code doesn't depend on a specific ADO.NET provider's exception shape.
- Known inconsistency: `IDbExceptionClassifier` is implemented by `SqlServerExceptionClassifier` (checks SQL Server error codes 2601/2627), even though the live provider is SQLite — check which classifier is actually wired up before relying on constraint-violation detection. Also, `appsettings.json`'s `SQLiteConnectionString` key is not read by the DI setup; the DB path is hardcoded in `Infrastructure/DependencyInjection.cs`.

### Authentication / Authorization

- JWT bearer auth (`JwtSettings`: `SecretKey`, `Issuer`, `Audience`, `ExpirationMinutes` in config). Tokens issued by `JwtService` (`Application/Services/Authorization`), HMAC-SHA256, with `ClaimTypes.NameIdentifier` + `jti` claims.
- `AuthController` (`api/Auth`, `[AllowAnonymous]`) exposes `login`/`register`; `AuthService` verifies passwords via `PasswordHasherService` (wraps ASP.NET Core Identity's `PasswordHasher<User>` — full Identity system is not used, just the hasher).
- Fallback authorization policy `"ExistingUser"` applies globally: authenticated + a custom `ExistingUserHandler` re-checks the user still exists in the DB by the JWT's `NameIdentifier` claim. Controllers use `[Authorize(Policy = "ExistingUser")]`; specific actions opt out with `[AllowAnonymous]` (e.g. account creation, all of `ProductsController`).
- Swagger UI accepts a raw JWT (no `Bearer ` prefix) when testing endpoints locally.
- The `JwtSettings.SecretKey` in `appsettings.json` is a real-looking value committed to source control — treat as a local-dev-only convenience, not something to replicate for other environments.

### Vendor integration

- `VendorApiClientBase` resolves a named `HttpClient` (via `IHttpClientFactory`) per vendor and reads per-vendor config (`VendorSettings:VendorDetails`: `Name`, `ApiUrl`, `TimeoutSeconds`, endpoint templates). Concrete clients (`FakeStoreAccountsApiClient`, `FakeStoreProductsApiClient`) implement only what they need.
- Vendor wire-format contracts (`Infrastructure/Apis/Contracts`) are kept separate from Application DTOs; dedicated mappers translate between them so vendor API shape never leaks into business logic. Only FakeStore is implemented today, though the `Vendors` enum / `DefaultVendor` config is built to support more.
- Each vendor `HttpClient` gets a standard resilience handler (`Microsoft.Extensions.Http.Resilience`): 10s per-attempt timeout, 3 retries with 2s exponential backoff, ~36s total timeout, plus a circuit breaker (50% failure ratio, min throughput 10, 30s sampling window, 15s break).

### Mapping

No AutoMapper — mapping is manual via static extension-method classes named `{Domain}Mappers.cs`, one set per layer boundary: `API/Mappers` (API contract ↔ Application DTO), `Application/Mappers` (Application DTO ↔ Entity), `Infrastructure/Mappers` (vendor wire format ↔ Application DTO).

### CQRS-like repositories

Per-domain interfaces in `Application/Interfaces/CommandsQueries` (`I{Domain}Commands`/`I{Domain}Queries`), implemented in `Infrastructure/Repositories/{Domain}/`. Queries use `AsNoTracking`; Commands wrap EF `DbUpdateException` into domain exceptions via `IDbExceptionClassifier`. This is a read/write split at the repository level, not full CQRS — no mediator (no MediatR).

### Cross-cutting middleware (in `Program.cs`)

- Logging: Serilog to console + rolling daily file (`Logs/log-.txt`); EF Core/Microsoft categories quieted to `Warning`.
- Rate limiting: fixed-window limiter (`Microsoft.AspNetCore.RateLimiting`), 100 permits/sec, queue 10, applied globally via `.RequireRateLimiting("fixed")`.
- Global exception handling: single `UseExceptionHandler` maps `KeyNotFoundException`→404, `ArgumentException`→400, else→500, JSON `{ message }` body. No separate custom middleware class.
- Validation: Data Annotations on `Api*Request` contracts (`[Required]`, `[Range]`, etc.), validated by the default `[ApiController]` model-binding pipeline — no FluentValidation.
- `UseDefaultServiceProvider` with `ValidateOnBuild`/`ValidateScopes` enabled — DI misconfiguration fails fast at startup.

## Testing conventions

- xUnit + Moq + AutoFixture (`AutoFixture.AutoMoq`, `_fixture.Freeze<Mock<T>>()` to share mocks between arrange/assert) + FluentAssertions.
- No real or in-memory database — services are tested against mocked `Commands`/`Queries`/`ApiClient` interfaces only. Vendor clients are tested against a stubbed `HttpMessageHandler`.
- One test file per domain (`AccountsTests.cs`, `OrdersTests.cs`, `ProductsTests.cs`), containing multiple `*ServiceTests` classes (one per use-case service). Plus `FakeStoreApiClientTests.cs` and `ConfigurationTests.cs` (config binding / `IOptionsMonitor`).
- Naming convention: `MethodName_Scenario_ExpectedOutcome`, e.g. `CreateAsync_VendorReturnsIdZero_ThrowsInvalidOperationException_AndNeverPersistsLocally`.

## Identity model

`User` (Users table) is the sole owner of login credentials (`Username`/`PasswordHash`), created/verified via `AuthController.register`/`login`. `Account` is pure business data (`Id`, `Email`, `Orders`) with no credential fields of its own — it's keyed 1:1 by the same id as the authenticated `User.Id`. Every Account/Order controller action derives that id from the JWT (`ClaimTypes.NameIdentifier`, parsed as `int`) rather than accepting it in the request body. Don't reintroduce `Username`/`Password` fields on `Account`; that duplicates what `User` already owns.

**Registration and account provisioning are one flow, not two.** There is no standalone `POST /api/Accounts` create endpoint — `AuthController.register` (`{ username, password, email }`) creates the `User` row *and* queues a `CreateAccount` job (`AuthService.RegisterAsync` → `IJobCommands`) using the new user's id, so `JobProcessingBackgroundService` provisions the vendor-linked `Account` asynchronously right after signup. `ICreateAccountService`/`CreateAccountService` still exist and are still the thing that actually talks to the vendor and persists the `Account` row — they're just invoked from the job pipeline now, not from a controller action. Don't re-add a client-facing create-account endpoint; that was the redundant duplicate of registration this design replaced.

Account/Order mutation services (`Create/Update/Delete/Execute/GetOrderService`, `Update/DeleteAccountService`) all guard against a not-yet-provisioned or deleted `Account` via the shared `IAccountExistenceGuard`/`AccountExistenceGuard` (`Application/Services/Account`), rather than each duplicating its own existence check — needed because Account provisioning is asynchronous (job queue), so a valid JWT doesn't guarantee the `Account` row exists yet. Reuse the guard for new Account/Order use cases instead of writing another private `CheckAccountExists`.
