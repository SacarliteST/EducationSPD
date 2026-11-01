---
name: focused-feature-flow
description: Implement or revise an EducationSPD feature with narrow analysis, targeted iteration checks, one complete final test run, formatting, final build, and a task-scoped commit.
---

# Focused feature flow

1. Read `AGENTS.md` and the concrete requirement. Inspect only the affected feature and one similar slice; do not scan the whole solution without a specific dependency.
2. Check `git status`, define the expected API behavior, layers and acceptance criteria, and keep unrelated work out of scope.
3. Use `rg` to trace the direct route, service, repository, entity and tests. Stop exploring when the local pattern and call chain are clear.
4. Implement the production path as one coherent batch. Add focused tests after the behavior stabilizes, unless a minimal regression test is needed first.
5. Test observable API behavior, persisted state and business invariants instead of private implementation steps.

## Validation ladder

- During implementation, build an affected project or run a relevant test filter only when it can answer the current question.
- If a test fails, rerun only that test while fixing it.
- After the task is fully implemented, build once and run the complete test suite exactly once.
- After the tests, run `dotnet format EducationSPD.sln --no-restore`, then run one final `dotnet build EducationSPD.sln --no-restore`.
- Do not rerun tests only because formatting completed and do not spend time reviewing a formatter-only diff.
- Check only `git status` to avoid staging unrelated work, stage task-scoped files, and create the task commit without another broad analysis pass.

Do not start the Web host, PostgreSQL containers, migrations, generation, dependency installation, push, deploy or cross-repository work unless the task requires it. Stop only processes and containers started by the agent. Report unavailable infrastructure honestly.

