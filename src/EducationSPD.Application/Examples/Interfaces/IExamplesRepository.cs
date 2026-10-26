using EducationSPD.Application.Common;
using EducationSPD.Domain.Examples;

namespace EducationSPD.Application.Examples;

/// <summary>Хранилище примеров.</summary>
public interface IExamplesRepository : IBaseRepository<Example, Guid>
{
}
