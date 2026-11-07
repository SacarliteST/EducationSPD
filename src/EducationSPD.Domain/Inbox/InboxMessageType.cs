namespace EducationSPD.Domain.Inbox;

/// <summary>Тип интеграционного сообщения в durable inbox.</summary>
public enum InboxMessageType : short
{
    /// <summary>Изменение каталога навыков учебного модуля.</summary>
    SkillCatalogChanged = 1,

    /// <summary>Финализация учебного эпизода.</summary>
    LearningEpisodeFinalized = 2
}
