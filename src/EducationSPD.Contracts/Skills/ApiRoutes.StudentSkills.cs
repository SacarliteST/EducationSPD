namespace EducationSPD.Contracts;

public static partial class ApiRoutes
{
    /// <summary>Маршруты чтения состояния навыков студентов.</summary>
    public static class StudentSkills
    {
        /// <summary>Состояния навыков студента внутри одного модуля.</summary>
        public const string List = PrefixV1 + "/modules/{moduleKey}/students/{studentId:guid}/skills";
    }
}
