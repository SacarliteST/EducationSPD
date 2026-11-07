namespace EducationSPD.Domain.Skills;

/// <summary>Происхождение навыка в каталоге учебного модуля.</summary>
public enum SkillOrigin : short
{
    /// <summary>Системный навык, определённый разработчиками модуля.</summary>
    System = 1,

    /// <summary>Навык, заявленный преподавателем.</summary>
    TeacherDefined = 2
}
