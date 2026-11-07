namespace EducationSPD.Infrastructure.Models;

internal sealed class RecalculationStudentModule
{
    public Guid StudentId { get; set; }

    public string ModuleKey { get; set; } = string.Empty;
}
