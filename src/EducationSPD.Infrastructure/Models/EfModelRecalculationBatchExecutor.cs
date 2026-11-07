using EducationSPD.Application.Episodes;
using EducationSPD.Application.Diagnostics;
using EducationSPD.Application.Models;
using EducationSPD.Domain.Models;
using EducationSPD.Infrastructure.Configuration;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EducationSPD.Infrastructure.Models;

internal sealed class EfModelRecalculationBatchExecutor : IModelRecalculationBatchExecutor
{
    private readonly AppDbContext dbContext;
    private readonly IStudentModuleLock studentModuleLock;
    private readonly RecalculationOptions options;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<EfModelRecalculationBatchExecutor> logger;

    public EfModelRecalculationBatchExecutor(
        AppDbContext dbContext,
        IStudentModuleLock studentModuleLock,
        IOptions<RecalculationOptions> options,
        TimeProvider timeProvider,
        ILogger<EfModelRecalculationBatchExecutor> logger)
    {
        this.dbContext = dbContext;
        this.studentModuleLock = studentModuleLock;
        this.options = options.Value;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        if (!await TryAcquireLockAsync(cancellationToken))
        {
            await transaction.CommitAsync(cancellationToken);
            return false;
        }

        var model = await dbContext.ModelVersions
            .SingleOrDefaultAsync(
                version => version.Status == ModelVersionStatus.Recalculating,
                cancellationToken);
        if (model is null)
        {
            model = await dbContext.ModelVersions
                .Where(version => version.Status == ModelVersionStatus.Draft)
                .OrderBy(version => version.Version)
                .FirstOrDefaultAsync(cancellationToken);
            if (model is null)
            {
                await transaction.CommitAsync(cancellationToken);
                return false;
            }

            model.StartRecalculation(timeProvider.GetUtcNow());
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Начат полный пересчёт версии модели {ModelVersion}", model.Version);
        }

        var pairs = await LoadNextPairsAsync(model, cancellationToken);
        if (pairs.Count == 0)
        {
            await ActivateAsync(model, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }

        foreach (var pair in pairs)
        {
            await studentModuleLock.AcquireAsync(pair.StudentId, pair.ModuleKey, cancellationToken);
            await RebuildStudentModuleAsync(model, pair, cancellationToken);
            model.AdvanceRecalculation(pair.ModuleKey, pair.StudentId);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        EducationSpdTelemetry.RecordModelRecalculationProgress(pairs.Count);
        logger.LogInformation(
            "Пересчитана порция версии модели {ModelVersion}: {PairCount} пар студент-модуль",
            model.Version,
            pairs.Count);
        return true;
    }

    private async Task<bool> TryAcquireLockAsync(CancellationToken cancellationToken) =>
        await dbContext.Database
            .SqlQueryRaw<bool>(
                "SELECT pg_try_advisory_xact_lock(hashtext('education-spd-model-recalculation')) AS \"Value\"")
            .SingleAsync(cancellationToken);

    private Task<List<RecalculationStudentModule>> LoadNextPairsAsync(
        ModelVersion model,
        CancellationToken cancellationToken)
    {
        if (model.RecalculationCursorModuleKey is null)
        {
            return dbContext.RecalculationStudentModules
                .FromSqlInterpolated($"""
                    SELECT student_id, module_key
                    FROM "LearningEpisodes"
                    GROUP BY module_key, student_id
                    ORDER BY module_key, student_id
                    LIMIT {options.BatchSize}
                    """)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        return dbContext.RecalculationStudentModules
            .FromSqlInterpolated($"""
                SELECT student_id, module_key
                FROM "LearningEpisodes"
                WHERE (module_key, student_id) >
                    ({model.RecalculationCursorModuleKey}, {model.RecalculationCursorStudentId!.Value})
                GROUP BY module_key, student_id
                ORDER BY module_key, student_id
                LIMIT {options.BatchSize}
                """)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private async Task RebuildStudentModuleAsync(
        ModelVersion model,
        RecalculationStudentModule pair,
        CancellationToken cancellationToken)
    {
        var history = await dbContext.LearningEpisodes
            .AsNoTracking()
            .Include(episode => episode.Observations)
            .Where(episode => episode.StudentId == pair.StudentId
                && episode.ModuleKey == pair.ModuleKey)
            .OrderBy(episode => episode.FinalizedAt)
            .ThenBy(episode => episode.EpisodeId)
            .ToListAsync(cancellationToken);
        var parameters = model.GetParameters();
        var skillIds = history
            .SelectMany(episode => episode.Observations)
            .Select(observation => observation.SkillId)
            .Distinct()
            .ToArray();

        foreach (var skillId in skillIds)
        {
            var mastery = parameters.InitialMasteryProbability;
            var count = 0;
            DateTimeOffset? lastObservedAt = null;
            foreach (var episode in history)
            {
                var observation = episode.Observations.SingleOrDefault(item => item.SkillId == skillId);
                if (observation is null)
                {
                    continue;
                }

                mastery = BktCalculator.Calculate(mastery, observation.Outcome, observation.Source, parameters);
                count++;
                lastObservedAt = episode.FinalizedAt;
            }

            var state = await dbContext.StudentSkillStates.SingleOrDefaultAsync(
                item => item.StudentId == pair.StudentId
                    && item.ModuleKey == pair.ModuleKey
                    && item.SkillId == skillId
                    && item.ModelVersion == model.Version,
                cancellationToken);
            if (state is null)
            {
                state = StudentSkillState.Create(pair.StudentId, pair.ModuleKey, skillId, model.Version);
                await dbContext.StudentSkillStates.AddAsync(state, cancellationToken);
            }

            state.Rebuild(mastery, count, lastObservedAt!.Value, timeProvider.GetUtcNow());
        }
    }

    private async Task ActivateAsync(ModelVersion model, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var activeModel = await dbContext.ModelVersions.SingleAsync(
            version => version.Status == ModelVersionStatus.Active,
            cancellationToken);
        activeModel.Archive(now);
        await dbContext.SaveChangesAsync(cancellationToken);

        model.Activate(now);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Завершён пересчёт и активирована версия модели {ModelVersion}; предыдущая версия {PreviousVersion} архивирована",
            model.Version,
            activeModel.Version);
        EducationSpdTelemetry.RecordProjectionRecalculation("model_activated");
    }
}
