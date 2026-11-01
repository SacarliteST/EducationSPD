---
name: unsandboxed-codegen
description: Run EducationSPD code-generation workflows outside the sandbox. Use for EF migrations, Swagger or OpenAPI export, generated clients, scaffolding, and commands that create or replace generated artifacts.
---

# Unsandboxed codegen

Never run a code generator or a prerequisite process used to produce its input inside the sandbox.

- Identify the established command and generated outputs before running it.
- Run the generator with `sandbox_permissions: require_escalated` and a narrowly scoped approval or reusable command prefix.
- Run prerequisite hosts or exporters outside the sandbox when they participate in artifact generation.
- If escalation is rejected or unavailable, stop and report that generation was not performed. Do not retry inside the sandbox.
- Do not hand-edit generated artifacts when they should be regenerated from their source.
- After generation, inspect the generated diff and run the smallest relevant build or contract check.
- Stop any temporary host or service started for generation.

This skill changes only where generation runs. It does not grant permission to deploy, push, commit or modify unrelated files.

