# AGENTS.md

Инструкции для агентской разработки backend EducationSPD.

## Репозиторные skills

Перед работой загружать минимальный подходящий набор инструкций:

- [`focused-feature-flow`](.agents/skills/focused-feature-flow/SKILL.md) — экономный цикл реализации, таргетные проверки и один полный финальный прогон;
- [`education-spd-development`](.agents/skills/education-spd-development/SKILL.md) — слои, API, EF Core и интеграционные тесты EducationSPD;
- [`human-readable-code`](.agents/skills/human-readable-code/SKILL.md) — общий стандарт простого и понятного кода;
- [`csharp-readable-implementation`](.agents/skills/csharp-readable-implementation/SKILL.md) — правила реализации на C#, EF Core и в сервисном слое;
- [`unsandboxed-codegen`](.agents/skills/unsandboxed-codegen/SKILL.md) — обязательный процесс для миграций, OpenAPI и другой генерации вне песочницы.

При реализации backend-фичи обычно применяются первые четыре skill. `unsandboxed-codegen` добавляется только при генерации артефактов. Явные правила этого файла имеют приоритет над общими рекомендациями skills.

## Архитектура

- `EducationSPD.Domain` содержит сущности и доменные инварианты.
- `EducationSPD.Application` содержит сценарии, интерфейсы и feature-oriented application types.
- `EducationSPD.Contracts` содержит публичные DTO и маршруты.
- `EducationSPD.Infrastructure` содержит EF Core, конфигурации, миграции и реализации репозиториев.
- `EducationSPD.Web` содержит Minimal API, mappings, validators, exception handling и composition root.
- `EducationSPD.Tests` проверяет HTTP-сценарии на PostgreSQL Testcontainers.

Использовать существующую фичу `Examples` как образец структуры, пока в проекте не закреплён новый шаблон.

## Базовые правила

- Один тип — один файл; код группируется по фичам.
- Публичные контракты и API документируются XML-комментариями на русском.
- Маршруты хранятся в `EducationSPD.Contracts.ApiRoutes`.
- Изменения схемы выполняются новыми EF migrations.
- Генерация выполняется только вне песочницы.
- Не включать чужие незакоммиченные изменения в коммит.
- Не выполнять push или deploy без явной команды пользователя.

