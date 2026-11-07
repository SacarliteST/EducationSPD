---
name: education-spd-development
description: Develop EducationSPD backend features using its layered Domain, Application, Contracts, Infrastructure and ASP.NET Core Controllers structure with EF Core and PostgreSQL integration tests.
---

# EducationSPD development

Read `AGENTS.md` first. Use the existing `Examples` feature only as a reference for
layer boundaries and test infrastructure. Its Minimal API transport is legacy and
must not be copied into new features.

## Route the change

- `src/EducationSPD.Domain`: entities and domain invariants without dependencies on outer layers.
- `src/EducationSPD.Application`: application services, commands, interfaces, feature DTOs and expected application exceptions.
- `src/EducationSPD.Contracts`: public request/response DTOs and centralized `ApiRoutes`.
- `src/EducationSPD.Infrastructure`: EF Core context, configurations, migrations and repository implementations.
- `src/EducationSPD.Web`: ASP.NET Core Controllers, mappings, validators, exception handling and composition.
- `tests/EducationSPD.Tests`: HTTP integration tests against PostgreSQL Testcontainers.

Keep code grouped by feature. Put public wire contracts in `Contracts/<Feature>/Dtos`,
application interfaces and exceptions inside their feature, controllers, mappings
and validators inside the corresponding Web feature, and EF configuration in
Infrastructure.

## HTTP transport

Implement EducationSPD HTTP API with ASP.NET Core Controllers. New endpoints must
use classes derived from `ControllerBase` with `[ApiController]`; do not implement
them through `MapGet`, `MapPost`, `MapGroup` or custom Minimal API endpoint groups.

Controllers are transport adapters only: accept and validate the request, map it
to an application input, call one application use case and map the result. Keep
business rules, EF queries and Kafka processing out of controllers.

Keep routes centralized in `EducationSPD.Contracts.ApiRoutes` and reference those
constants from route attributes. Register controllers through `AddControllers()`
and map them through `MapControllers()`.

When an existing Minimal API endpoint is changed as part of a requested feature,
migrate that endpoint to a controller if this can be done without changing its
public contract. Do not perform an unrelated full transport rewrite implicitly.

## API and persistence

Keep routes centralized in `EducationSPD.Contracts.ApiRoutes`. Document public APIs and contracts in Russian XML comments. Describe endpoint responses in OpenAPI and preserve established validation/error mapping unless the requirement explicitly changes the contract.

Use `Guid` identifiers, `AsNoTracking()` for read-only queries, stable pagination, cancellation tokens and EF migrations for schema changes. Do not replace migrations with `EnsureCreated` and do not rewrite applied migrations.

Generate migrations and other artifacts through the local `$unsandboxed-codegen` skill. Follow `$focused-feature-flow`, `$human-readable-code` and `$csharp-readable-implementation` for implementation and validation.
