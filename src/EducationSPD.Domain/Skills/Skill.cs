namespace EducationSPD.Domain.Skills;

/// <summary>Локальная проекция определения навыка учебного модуля.</summary>
public sealed class Skill
{
    /// <summary>Максимальная длина ключа модуля.</summary>
    public const int ModuleKeyMaxLength = 100;

    /// <summary>Максимальная длина идентификатора навыка.</summary>
    public const int SkillIdMaxLength = 100;

    /// <summary>Максимальная длина названия навыка.</summary>
    public const int NameMaxLength = 300;

    /// <summary>Максимальная длина описания навыка.</summary>
    public const int DescriptionMaxLength = 2000;

    private Skill()
    {
    }

    private Skill(
        Guid id,
        string moduleKey,
        string skillId,
        string name,
        string? description,
        SkillOrigin origin,
        bool isDeleted,
        DateTimeOffset occurredAt,
        Guid sourceEventId)
    {
        Id = id;
        ModuleKey = moduleKey;
        SkillId = skillId;
        Name = name;
        Description = description;
        Origin = origin;
        IsDeleted = isDeleted;
        OccurredAt = occurredAt;
        SourceEventId = sourceEventId;
    }

    /// <summary>Внутренний идентификатор проекции навыка.</summary>
    public Guid Id { get; private set; }

    /// <summary>Стабильный ключ учебного модуля.</summary>
    public string ModuleKey { get; private set; } = string.Empty;

    /// <summary>Стабильный идентификатор навыка внутри модуля.</summary>
    public string SkillId { get; private set; } = string.Empty;

    /// <summary>Отображаемое название навыка.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Необязательное описание навыка.</summary>
    public string? Description { get; private set; }

    /// <summary>Происхождение навыка.</summary>
    public SkillOrigin Origin { get; private set; }

    /// <summary>Признак мягкого удаления навыка.</summary>
    public bool IsDeleted { get; private set; }

    /// <summary>Время последнего применённого изменения каталога.</summary>
    public DateTimeOffset OccurredAt { get; private set; }

    /// <summary>Идентификатор события, сформировавшего текущее состояние.</summary>
    public Guid SourceEventId { get; private set; }

    /// <summary>Создаёт первоначальную проекцию навыка.</summary>
    public static Skill Create(
        Guid id,
        string moduleKey,
        string skillId,
        string name,
        string? description,
        SkillOrigin origin,
        bool isDeleted,
        DateTimeOffset occurredAt,
        Guid sourceEventId)
    {
        ValidateIdentity(id, moduleKey, skillId);
        ValidateState(name, description, origin, occurredAt, sourceEventId);

        return new Skill(
            id,
            moduleKey,
            skillId,
            name,
            description,
            origin,
            isDeleted,
            occurredAt,
            sourceEventId);
    }

    /// <summary>
    /// Применяет более новое состояние каталога, сохраняя идентичность навыка.
    /// </summary>
    /// <returns><see langword="true"/>, если состояние было обновлено.</returns>
    public bool ApplyChange(
        string name,
        string? description,
        SkillOrigin origin,
        bool isDeleted,
        DateTimeOffset occurredAt,
        Guid sourceEventId)
    {
        ValidateState(name, description, origin, occurredAt, sourceEventId);

        if (!IsNewerThanCurrentState(occurredAt, sourceEventId))
        {
            return false;
        }

        Name = name;
        Description = description;
        Origin = origin;
        IsDeleted = isDeleted;
        OccurredAt = occurredAt;
        SourceEventId = sourceEventId;
        return true;
    }

    private bool IsNewerThanCurrentState(DateTimeOffset occurredAt, Guid sourceEventId)
    {
        var timestampComparison = occurredAt.CompareTo(OccurredAt);
        return timestampComparison > 0
            || timestampComparison == 0 && sourceEventId.CompareTo(SourceEventId) > 0;
    }

    private static void ValidateIdentity(Guid id, string moduleKey, string skillId)
    {
        RequireGuid(id, nameof(id));
        RequireIdentifier(
            moduleKey,
            nameof(moduleKey),
            ModuleKeyMaxLength,
            static character => IsLowercaseLatinLetter(character) || IsAsciiDigit(character) || character == '-');
        RequireIdentifier(
            skillId,
            nameof(skillId),
            SkillIdMaxLength,
            static character => IsLowercaseLatinLetter(character)
                || IsAsciiDigit(character)
                || character is '-' or '_' or '.');
    }

    private static void ValidateState(
        string name,
        string? description,
        SkillOrigin origin,
        DateTimeOffset occurredAt,
        Guid sourceEventId)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > NameMaxLength)
        {
            throw new ArgumentException($"Название навыка должно содержать от 1 до {NameMaxLength} символов.", nameof(name));
        }

        if (description?.Length > DescriptionMaxLength)
        {
            throw new ArgumentException($"Описание навыка не может быть длиннее {DescriptionMaxLength} символов.", nameof(description));
        }

        if (!Enum.IsDefined(origin))
        {
            throw new ArgumentOutOfRangeException(nameof(origin), "Происхождение навыка не поддерживается.");
        }

        if (occurredAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Время изменения каталога должно быть указано в UTC.", nameof(occurredAt));
        }

        RequireGuid(sourceEventId, nameof(sourceEventId));
    }

    private static void RequireIdentifier(
        string value,
        string parameterName,
        int maxLength,
        Func<char, bool> isAllowedCharacter)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength)
        {
            throw new ArgumentException($"Значение должно содержать от 1 до {maxLength} символов.", parameterName);
        }

        if (!value.All(isAllowedCharacter))
        {
            throw new ArgumentException("Значение содержит недопустимые символы.", parameterName);
        }
    }

    private static void RequireGuid(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Идентификатор не может быть пустым UUID.", parameterName);
        }
    }

    private static bool IsLowercaseLatinLetter(char character) => character is >= 'a' and <= 'z';

    private static bool IsAsciiDigit(char character) => character is >= '0' and <= '9';
}
