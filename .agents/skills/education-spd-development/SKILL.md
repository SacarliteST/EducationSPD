---
name: education-spd-development
description: Develop EducationSPD backend features using its layered Domain, Application, Contracts, Infrastructure and Minimal API structure with EF Core and PostgreSQL integration tests.
---

# EducationSPD development

Read `AGENTS.md` first. Use the existing `Examples` feature as the current structural reference until the repository establishes another pattern.

## Route the change

- `src/EducationSPD.Domain`: entities and domain invariants without dependencies on outer layers.
- `src/EducationSPD.Application`: application services, commands, interfaces, feature DTOs and expected application exceptions.
- `src/EducationSPD.Contracts`: public request/response DTOs and centralized `ApiRoutes`.
- `src/EducationSPD.Infrastructure`: EF Core context, configurations, migrations and repository implementations.
- `src/EducationSPD.Web`: Minimal API endpoint groups, mappings, validators, exception handling and composition.
- `tests/EducationSPD.Tests`: HTTP integration tests against PostgreSQL Testcontainers.

Keep code grouped by feature. Put public wire contracts in `Contracts/<Feature>/Dtos`, application interfaces and exceptions inside their feature, endpoint mappings and validators beside the endpoint group, and EF configuration in Infrastructure.

## API and persistence

Keep routes centralized in `EducationSPD.Contracts.ApiRoutes`. Document public APIs and contracts in Russian XML comments. Describe endpoint responses in OpenAPI and preserve established validation/error mapping unless the requirement explicitly changes the contract.

Use `Guid` identifiers, `AsNoTracking()` for read-only queries, stable pagination, cancellation tokens and EF migrations for schema changes. Do not replace migrations with `EnsureCreated` and do not rewrite applied migrations.

Generate migrations and other artifacts through the local `$unsandboxed-codegen` skill. Follow `$focused-feature-flow`, `$human-readable-code` and `$csharp-readable-implementation` for implementation and validation.

