using EducationSPD.Application.Models;
using EducationSPD.Domain.Models;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Infrastructure.Models;

internal sealed class EfStudentSkillStateRepository(AppDbContext dbContext)
    : IStudentSkillStateRepository
{
    public Task<StudentSkillState?> FindAsync(
        Guid studentId,
        string moduleKey,
        Guid skillId,
        int modelVersion,
        CancellationToken cancellationToken) =>
        dbContext.StudentSkillStates.SingleOrDefaultAsync(
            state => state.StudentId == studentId
                && state.ModuleKey == moduleKey
                && state.SkillId == skillId
                && state.ModelVersion == modelVersion,
            cancellationToken);

    public async Task AddAsync(StudentSkillState state, CancellationToken cancellationToken) =>
        await dbContext.StudentSkillStates.AddAsync(state, cancellationToken);
}
