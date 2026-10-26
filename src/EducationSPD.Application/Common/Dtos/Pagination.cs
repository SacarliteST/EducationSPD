namespace EducationSPD.Application.Common;

/// <summary>
/// Параметры постраничного запроса.
/// </summary>
public sealed record Pagination(int Offset, int Limit);
