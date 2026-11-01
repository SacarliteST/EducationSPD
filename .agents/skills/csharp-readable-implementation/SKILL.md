---
name: csharp-readable-implementation
description: Write readable C# and .NET code in EducationSPD using ordinary constructors, feature-oriented files, explicit application flow, restrained LINQ, and framework-aware validation.
---

# Readable C# implementation

Apply `$human-readable-code` together with `$education-spd-development`.

## Types and layout

- Use one meaningful type per file and match the file name to the type.
- Place code in the owning feature folder. Put DTOs, interfaces and exceptions in their designated feature subfolders instead of unrelated common buckets.
- Use ordinary constructors for services and handlers, not primary constructors.
- Constructor bodies should normally only assign dependencies. Do not add repeated null guards for services created by dependency injection.
- Validate options once through startup or options validation rather than in every consumer constructor.
- Use records for immutable contracts or value-like data, not automatically for services or stateful entities.

## Control flow and data access

- Keep application services as readable use-case orchestration and domain decisions close to domain state.
- Follow the repository's established expected-error mechanism; do not introduce a second competing result or exception framework for one feature.
- Keep LINQ readable. Split a long query into named stages when it exposes intent, but do not materialize data merely to simplify syntax.
- Use `AsNoTracking()` for read-only EF queries and pass `CancellationToken` through asynchronous calls.
- Prefer EF Core for ordinary CRUD and queries. Use raw SQL only for a database primitive EF cannot express adequately and make the reason visible.
- Avoid fire-and-forget work unless an established background or outbox mechanism owns it.

## Contracts, logs and documentation

- Keep nullable request DTOs separate from non-null domain state.
- Add Russian XML documentation to public contracts and APIs; internal comments must explain reasoning rather than ceremony.
- Use structured logging with stable identifiers and Russian messages. Never log secrets, tokens, connection strings, raw unsafe database errors or full request bodies.
- Do not suppress compiler or nullable warnings merely to make a build pass.

Write tests around public responses, persisted state and visible side effects. Do not expose internals only to make them easier to test.

