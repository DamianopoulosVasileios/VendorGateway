# VendorGateway — Architecture

## Overview

VendorGateway is a **Clean Architecture** solution with an API proejct that acts as a gateway between external clients and one or more third-party vendor systems (currently FakeStore). It exposes a REST API for managing Accounts, Orders, and Products, synchronizes data with the vendor, and persists its own local copy of that data.

The system is organized into four projects, each with a single, clearly bounded responsibility:

| Project | Responsibility |
|---|---|
| `VendorGateway.API` | HTTP surface: controllers, request/response contracts, filters |
| `VendorGateway.Application` | Business logic: use-case services, domain entities, interfaces |
| `VendorGateway.Infrastructure` | Implementation details: EF Core persistence, vendor API clients, mappers |
| `VendorGateway.Tests` | Automated test coverage across the above |

Dependencies point inward only: `API` depends on `Application` (for use cases) and `Infrastructure` (for Dependency Injection only) ; `Infrastructure` depends on `Application`; `Application` depends on nothing else in the solution. This is what makes the architecture "clean" — business logic in `Application` has no knowledge of EF Core, HTTP, or any specific vendor; it only knows about interfaces it defines and expects someone else to implement.

```
                 ┌─────────────────────┐
                 │   VendorGateway.API  │   Controllers, Contracts, Filters
                 └──────────┬───────────┘
                            │ depends on
                            ▼
                 ┌─────────────────────┐
                 │ VendorGateway.       │   Services (use cases), Entities,
                 │ Application          │   DTOs, interfaces (Commands/Queries,
                 │                      │   ApiClient, Services)
                 └──────────▲───────────┘
                            │ implements interfaces defined above
                 ┌──────────┴───────────┐
                 │ VendorGateway.        │   EF Core (AppDbContext, migrations,
                 │ Infrastructure        │   repositories), vendor HTTP clients,
                 │                       │   exception classifiers, mappers
                 └───────────────────────┘
```

## Request flow

Every action follows the same shape, regardless of whether it targets an Account, Order, or Product:

1. **Controller** (`API/Controllers`) receives the HTTP request, bound to an `Api*Request` contract (`API/Contracts`).
2. The controller maps the `Api*Request` into whatever shape the `Application` layer expects, and calls a **use-case service** (`Application/Services/{Domain}/{Verb}{Domain}Service.cs`) through its interface (`Application/Interfaces/Services`).
3. The service performs **business rules and validation** — existence checks, discount calculations, state-transition guards — using **queries** to read data and **commands** to write it, both accessed only through interfaces (`Application/Interfaces/CommandsQueries`).
4. **Infrastructure** provides the concrete implementation of those interfaces:
   - `Repositories/{Domain}/{Domain}Commands.cs` / `{Domain}Queries.cs` — EF Core reads/writes against `AppDbContext`.
   - `Apis/FakeStore{Domain}ApiClient.cs` — HTTP calls to the vendor, translated via `Mappers/ApiAndFakeStoreAccountMappers.cs` (and equivalents) between the vendor's wire format and the Application layer's DTOs.
5. The service returns a result (or throws a domain exception — `KeyNotFoundException`, `InvalidOperationException`, etc.) back up to the controller, which maps it to an `Api*Response` and an appropriate HTTP status code.

**Example — registering and provisioning an Account:**

Registration and account creation are one flow, not two: signing up creates both your login credentials and your vendor-linked Account under the same id, so there's no separate "create account" call.

```
POST /api/Auth/register { username, password, email }
  → AuthController.RegisterAsync(RegisterUserRequest)
    → AuthService.RegisterAsync(...)
      → IAuthorizationCommands.RegisterUserAsync(...)   [Infrastructure: persists the User/credentials]
      → IJobCommands.CreateAsync(CreateAccount job)     [queues vendor-linked Account provisioning]
  ← 201 Created

# asynchronously, via JobProcessingBackgroundService:
  → ICreateAccountService.CreateAsync(CreateAccountRequest, id, ct)
    → IAccountsApiClient.CreateAsync(...)   [Infrastructure: calls FakeStore]
    → IAccountCommands.CreateAsync(...)     [Infrastructure: persists locally via EF Core]
```

Each use case gets its own service and interface (`ICreateAccountService`, `IUpdateOrderService`, `IExecuteOrderService`, etc.) rather than a single generic "AccountService" — this keeps each class focused on one action's rules and makes them independently testable.

## Asynchronous processing

Write actions (create/update/delete/execute) do not perform vendor calls or persistence synchronously within the HTTP request. Instead:

1. The controller serializes the request into a **job payload** and inserts a `Job` row (`Application/Jobs/Entities/JobEntities.cs`), then immediately returns **`202 Accepted`** — the request is *submitted*, not *completed*.
2. `JobProcessingBackgroundService` (`Application/Jobs`) polls the `Jobs` table on a fixed interval, atomically claims a batch of pending jobs, and dispatches each one to its corresponding use-case service based on `Job.Type`.
3. `FailedJobProcessingBackgroundService` handles retry/cleanup of jobs that failed during processing.
4. `SyncProductsFromVendorBackgroundService` runs the product catalog sync (`CreateProductService`) on its own schedule, independent of the general job queue.

This decouples API response time from vendor latency/availability — the client is acknowledged immediately, and the actual work happens out-of-band. There is currently no client-facing status/polling endpoint; job outcomes are tracked internally via `Job.Status` and `Job.ErrorMessage`.

## Persistence

- `AppDbContext` (`Infrastructure/Persistence`) is the single EF Core context for the solution, covering Accounts, Orders, OrderItems, Products, and Jobs.
- All entities implementing `IAuditable` have `CreatedAt`/`UpdatedAt` populated automatically by an override of `SaveChangesAsync` — no service or repository sets these manually.
- Schema changes are tracked via EF Core Migrations (`Infrastructure/Migrations`), applied automatically at startup via `Database.Migrate()`.
- Idempotent order creation is enforced at the database level via a unique constraint on `Order.IdempotencyKey`, supplied by the client via an `Idempotency-Key` header (`API/Filters/RequireIdempotencyKeyAttribute.cs`).
- Database-specific failure modes (e.g. unique constraint violations) are abstracted behind `IDbExceptionClassifier`, so repository code can react to "this was a duplicate" without depending on a specific ADO.NET provider's exception type.

## Vendor integration

- `VendorApiClientBase` provides shared HTTP-client resolution and per-vendor configuration (`VendorsConfiguration`, `VendorDetails`) so each vendor-specific client (`FakeStoreAccountsApiClient`, `FakeStoreProductsApiClient`) only implements the calls it needs.
- Vendor-specific request/response shapes (`Apis/Contracts`) are kept separate from the Application layer's own DTOs; dedicated mappers (`Infrastructure/Mappers`) translate between the two, so a vendor's API shape never leaks into business logic.

## Testing

`VendorGateway.Tests` covers each layer independently:

- Use-case services are tested in isolation with mocked commands/queries/API clients (xUnit + Moq + AutoFixture), verifying both the happy path and every thrown exception/edge case.
- Vendor API client behavior is verified against a stubbed `HttpMessageHandler`.
- Configuration binding (`VendorsConfiguration`, `IOptionsMonitor` wiring) has dedicated coverage to catch misconfiguration early.