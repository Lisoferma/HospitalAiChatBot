namespace HospitalAiChatBot.Source.Models.Llm.Giga;

/// <summary>
///     Версия API GigaChat
/// </summary>
public enum GigaChatApiScope
{
    /// <summary>
    ///     Доступ для физических лиц.
    /// </summary>
    Personal,

    /// <summary>
    ///     Доступ для ИП и юридических лиц по платным пакетам.
    /// </summary>
    BusinessToBusiness,

    /// <summary>
    ///     Доступ для ИП и юридических лиц по схеме pay-as-you-go.
    /// </summary>
    Corporate
}

public static class GigaChatApiScopeExtensions
{
    /// <summary>
    ///     Преобразование <see cref="GigaChatApiScope" /> в строку, корректную для запроса GigaChat API
    /// </summary>
    public static string ToApiRequestFormatString(this GigaChatApiScope scope)
    {
        return scope switch
        {
            GigaChatApiScope.Personal => "GIGACHAT_API_PERS",
            GigaChatApiScope.BusinessToBusiness => "GIGACHAT_API_B2B",
            GigaChatApiScope.Corporate => "GIGACHAT_API_CORP",
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope,
                $"Can't convert {nameof(GigaChatApiScope)} ({scope}) to string")
        };
    }
}