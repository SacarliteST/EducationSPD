using EducationSPD.Kafka.Contracts.V1;

namespace EducationSPD.Kafka.Contracts;

internal static class KafkaContractValidator
{
    private const int ModuleKeyMaxLength = 100;
    private const int SkillIdMaxLength = 100;

    public static void Validate(IKafkaContract message)
    {
        if (message.SchemaVersion != KafkaSchemaVersions.Version1)
        {
            throw Invalid($"Версия Kafka-схемы {message.SchemaVersion} не поддерживается.");
        }

        switch (message)
        {
            case SkillCatalogChanged catalogChanged:
                Validate(catalogChanged);
                break;
            case LearningEpisodeFinalized episodeFinalized:
                Validate(episodeFinalized);
                break;
            default:
                throw Invalid($"Тип Kafka-контракта {message.GetType().Name} не поддерживается.");
        }
    }

    private static void Validate(SkillCatalogChanged message)
    {
        RequireGuid(message.EventId, nameof(message.EventId));
        RequireModuleKey(message.ModuleKey);
        RequireSkillId(message.SkillId);
        RequireText(message.Name, nameof(message.Name), 300);

        if (message.Description?.Length > 2000)
        {
            throw Invalid("Description не может быть длиннее 2000 символов.");
        }

        RequireDefinedEnum(message.Origin, nameof(message.Origin));
        RequireUtc(message.OccurredAt, nameof(message.OccurredAt));
    }

    private static void Validate(LearningEpisodeFinalized message)
    {
        RequireGuid(message.EventId, nameof(message.EventId));
        RequireGuid(message.EpisodeId, nameof(message.EpisodeId));
        RequireGuid(message.StudentId, nameof(message.StudentId));
        RequireModuleKey(message.ModuleKey);
        RequireGuid(message.TaskId, nameof(message.TaskId));
        RequireUtc(message.FinalizedAt, nameof(message.FinalizedAt));

        if (message.SkillResults is not { Count: >= 1 and <= 100 })
        {
            throw Invalid("SkillResults должен содержать от 1 до 100 элементов.");
        }

        var skillIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var result in message.SkillResults)
        {
            if (result is null)
            {
                throw Invalid("SkillResults не может содержать null.");
            }

            RequireSkillId(result.SkillId);
            RequireDefinedEnum(result.Source, nameof(result.Source));
            RequireDefinedEnum(result.Outcome, nameof(result.Outcome));

            if (!skillIds.Add(result.SkillId))
            {
                throw Invalid($"SkillId '{result.SkillId}' повторяется внутри события.");
            }
        }
    }

    private static void RequireModuleKey(string value)
    {
        RequireIdentifier(
            value,
            nameof(SkillCatalogChanged.ModuleKey),
            ModuleKeyMaxLength,
            static character => IsLowercaseLatinLetter(character) || IsAsciiDigit(character) || character == '-');
    }

    private static void RequireSkillId(string value)
    {
        RequireIdentifier(
            value,
            nameof(SkillCatalogChanged.SkillId),
            SkillIdMaxLength,
            static character => IsLowercaseLatinLetter(character)
                || IsAsciiDigit(character)
                || character is '-' or '_' or '.');
    }

    private static void RequireIdentifier(
        string value,
        string fieldName,
        int maxLength,
        Func<char, bool> isAllowedCharacter)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength)
        {
            throw Invalid($"{fieldName} должен содержать от 1 до {maxLength} символов.");
        }

        if (!value.All(isAllowedCharacter))
        {
            throw Invalid($"{fieldName} содержит недопустимые символы.");
        }
    }

    private static void RequireText(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength)
        {
            throw Invalid($"{fieldName} должен содержать от 1 до {maxLength} символов.");
        }
    }

    private static void RequireGuid(Guid value, string fieldName)
    {
        if (value == Guid.Empty)
        {
            throw Invalid($"{fieldName} не может быть пустым UUID.");
        }
    }

    private static void RequireUtc(DateTimeOffset value, string fieldName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw Invalid($"{fieldName} должен содержать время UTC.");
        }
    }

    private static void RequireDefinedEnum<TEnum>(TEnum value, string fieldName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw Invalid($"{fieldName} содержит неизвестное значение.");
        }
    }

    private static bool IsLowercaseLatinLetter(char character) => character is >= 'a' and <= 'z';

    private static bool IsAsciiDigit(char character) => character is >= '0' and <= '9';

    private static KafkaContractValidationException Invalid(string message) => new(message);
}
