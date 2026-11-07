using System.Text.Json.Serialization;

namespace EducationSPD.Contracts.Skills;

/// <summary>Происхождение определения навыка.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<StudentSkillOrigin>))]
public enum StudentSkillOrigin
{
    /// <summary>Системный навык точного правила проверки.</summary>
    System = 1,

    /// <summary>Навык-тег, заявленный преподавателем.</summary>
    TeacherDefined = 2,
}
