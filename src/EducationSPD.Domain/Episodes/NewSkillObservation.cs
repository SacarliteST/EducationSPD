namespace EducationSPD.Domain.Episodes;

/// <summary>Данные нового неизменяемого наблюдения навыка.</summary>
public sealed record NewSkillObservation(
    Guid Id,
    Guid SkillId,
    SkillObservationSource Source,
    SkillObservationOutcome Outcome);
