using System.Text.Json.Serialization;

namespace HospitalAiChatBot.Models.Llm;

/// <summary>
///     Сообщение чата с LLM
/// </summary>
public class LlmChatMessage(string content, LlmChatMessageAuthorRole role = LlmChatMessageAuthorRole.User)
{
    /// <summary>
    ///     Роль автора сообщения
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    [JsonIgnore] public readonly LlmChatMessageAuthorRole Role = role;

    /// <summary>
    ///     Роль автора сообщения в виде строки для API
    /// </summary>
    [JsonPropertyName("role")]
    public string LlmChatMessageAuthorRoleApiString => Role.ToApiRequestFormatString();

    /// <summary>
    ///     Содержимое сообщения
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; init; } = content;
}