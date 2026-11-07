namespace EducationSPD.Web.Security;

/// <summary>
/// Настройки аутентификации доверенных сервисов.
/// </summary>
internal sealed class ServiceAuthenticationOptions
{
    public const string SectionName = "ServiceAuthentication";

    public string ServiceKey { get; set; } = string.Empty;
}
