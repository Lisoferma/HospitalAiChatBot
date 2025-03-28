namespace HospitalAiChatBot.Source.Models.Llm;

/// <summary>
///     Роль автора сообщения в чате с LLM
/// </summary>
public enum LlmChatMessageAuthorRole
{
    /// <summary>
    ///     Система. С помощью данной роли задаются роль, цели и иной контекст модели
    /// </summary>
    System,

    /// <summary>
    ///     Пользователь. С помощью данной роли передаются сообщения пользователя
    /// </summary>
    User,

    /// <summary>
    ///     Помощник. С помощью данной роли передаются ответы модели
    /// </summary>
    Assistant
}

public static class LlmChatMessageAuthorRoleExtensions
{
    /// <summary>
    ///     Преобразует <see cref="LlmChatMessageAuthorRole" /> в строку, корректную для тела запроса к API LLM
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string ToApiRequestFormatString(this LlmChatMessageAuthorRole role)
    {
        return role switch
        {
            LlmChatMessageAuthorRole.System => "system",
            LlmChatMessageAuthorRole.User => "user",
            LlmChatMessageAuthorRole.Assistant => "assistant",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role,
                $"Cannot convert {nameof(LlmChatMessageAuthorRole)} ({role}) to string")
        };
    }
}