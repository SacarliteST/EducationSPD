namespace EducationSPD.Contracts.System;

/// <summary>
/// Состояние HTTP-процесса EducationSPD.
/// </summary>
/// <param name="Status">Текущее состояние.</param>
public sealed record HealthResponse(string Status);
