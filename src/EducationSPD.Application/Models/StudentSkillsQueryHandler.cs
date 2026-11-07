namespace EducationSPD.Application.Models;

internal sealed class StudentSkillsQueryHandler : IStudentSkillsQueryHandler
{
    private readonly IModelVersionRepository modelVersionRepository;
    private readonly IStudentSkillStateReader stateReader;

    public StudentSkillsQueryHandler(
        IModelVersionRepository modelVersionRepository,
        IStudentSkillStateReader stateReader)
    {
        this.modelVersionRepository = modelVersionRepository;
        this.stateReader = stateReader;
    }

    public async Task<StudentSkillsPage> GetAsync(
        string moduleKey,
        Guid studentId,
        bool includeDeleted,
        int offset,
        int limit,
        CancellationToken cancellationToken)
    {
        var activeModel = await modelVersionRepository.GetActiveAsync(cancellationToken)
            ?? throw new InvalidOperationException("Активная версия математической модели не найдена.");
        var slice = await stateReader.ReadAsync(
            moduleKey,
            studentId,
            activeModel.Version,
            activeModel.InitialMasteryProbability,
            includeDeleted,
            offset,
            limit,
            cancellationToken);

        return new StudentSkillsPage(
            slice.Count,
            offset,
            limit,
            activeModel.Version,
            slice.Items);
    }
}
