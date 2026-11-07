namespace EducationSPD.Kafka.Contracts.V1;

/// <summary>Происхождение навыка в каталоге учебного модуля.</summary>
public enum SkillOrigin
{
    /// <summary>Системный навык, определённый разработчиками модуля.</summary>
    System = 1,

    /// <summary>Навык, заявленный преподавателем.</summary>
    TeacherDefined = 2
}
