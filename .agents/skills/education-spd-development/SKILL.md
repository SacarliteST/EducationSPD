---
name: education-spd-development
description: Develop EducationSPD backend features in its Domain, Application, Contracts, Infrastructure, Web and Host projects, using ASP.NET Core Controllers, EF Core and appropriately scoped HTTP tests.
---

# EducationSPD development

Read `AGENTS.md` first. Keep the executable composition root isolated in
`EducationSPD.Host`. Do not add demonstration or placeholder CRUD features to the
working layers; use the existing health endpoint for a basic host smoke test.

## Route the change

- `src/EducationSPD.Domain`: entities and domain invariants without dependencies on outer layers.
- `src/EducationSPD.Application`: application services, commands, interfaces, feature DTOs and expected application exceptions.
- `src/EducationSPD.Contracts`: public request/response DTOs and centralized `ApiRoutes`.
- `src/EducationSPD.Infrastructure`: EF Core context, configurations, migrations and repository implementations.
- `src/EducationSPD.Web`: ASP.NET Core Controllers, mappings, validators, exception handling and HTTP-layer registration.
- `src/EducationSPD.Host`: executable entry point and composition root that invokes each layer registration.
- `tests/EducationSPD.Tests`: HTTP tests through `WebApplicationFactory`; use PostgreSQL Testcontainers only for scenarios that exercise persistence.

Keep code grouped by feature. Put public wire contracts in `Contracts/<Feature>/Dtos`,
application interfaces and exceptions inside their feature, controllers, mappings
and validators inside the corresponding Web feature, and EF configuration in
Infrastructure.

## Compose the application

Each layer owns its registration extension:

- Application exposes `AddApplication()`;
- Infrastructure exposes `AddInfrastructure(configuration)`;
- Web exposes `AddWeb(configuration)`;
- Host calls these extensions and contains environment-level setup.

Keep `Program` limited to creating the builder, delegating to `Startup`, building
and running the application. Keep service composition and middleware ordering in
Host `Startup`. Store executable configuration and launch profiles in Host.

Web must not reference Infrastructure. Application and Domain must not reference
Web, Infrastructure or Host. Register a new implementation in the layer that owns
it instead of registering feature services directly in Host.

## HTTP transport

Implement EducationSPD HTTP API with ASP.NET Core Controllers. New endpoints must
use classes derived from `ControllerBase` with `[ApiController]`; do not implement
them through `MapGet`, `MapPost`, `MapGroup` or custom Minimal API endpoint groups.

Controllers are transport adapters only: accept and validate the request, map it
to an application input, call one application use case and map the result. Keep
business rules, EF queries and Kafka processing out of controllers.

Keep routes centralized in `EducationSPD.Contracts.ApiRoutes` and reference those
constants from route attributes. Web registers controllers through
`AddControllers()`; Host maps them through `MapControllers()`.

## API and persistence

Keep routes centralized in `EducationSPD.Contracts.ApiRoutes`. Document public APIs and contracts in Russian XML comments. Describe endpoint responses in OpenAPI and preserve established validation/error mapping unless the requirement explicitly changes the contract.

Use `Guid` identifiers, `AsNoTracking()` for read-only queries, stable pagination, cancellation tokens and EF migrations for schema changes. Do not replace migrations with `EnsureCreated` and do not rewrite applied migrations.

Generate migrations and other artifacts through the local `$unsandboxed-codegen` skill. Follow `$focused-feature-flow`, `$human-readable-code` and `$csharp-readable-implementation` for implementation and validation.
