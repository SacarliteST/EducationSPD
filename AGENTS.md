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
- `EducationSPD.Contracts` содержит публичные HTTP DTO и маршруты.
- `EducationSPD.Kafka.Contracts` содержит переиспользуемые wire-контракты Kafka
  и не зависит от Kafka SDK, EF Core или доменных сущностей.
- `EducationSPD.Infrastructure` содержит EF Core, конфигурации, миграции и реализации репозиториев.
- `EducationSPD.Web` содержит ASP.NET Core Controllers, mappings, validators,
  exception handling и регистрацию HTTP-слоя. Новые HTTP endpoint реализуются
  только через Controllers.
- `EducationSPD.Host` является единственной точкой входа и composition root:
  `Startup` собирает регистрации слоёв и настраивает HTTP-конвейер.
- `EducationSPD.Tests` проверяет HTTP-сценарии; сценарии с хранением данных
  выполняются на PostgreSQL Testcontainers.

## Базовые правила

- Один тип — один файл; код группируется по фичам.
- Публичные контракты и API документируются XML-комментариями на русском.
- Маршруты хранятся в `EducationSPD.Contracts.ApiRoutes`.
- Изменения схемы выполняются новыми EF migrations.
- Генерация выполняется только вне песочницы.
- Не включать чужие незакоммиченные изменения в коммит.
- Не выполнять push или deploy без явной команды пользователя.
