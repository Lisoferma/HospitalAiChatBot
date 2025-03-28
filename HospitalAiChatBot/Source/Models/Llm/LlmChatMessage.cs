using System.Text.Json.Serialization;

namespace HospitalAiChatBot.Source.Models.Llm;

/// <summary>
///     Сообщение чата с LLM
/// </summary>
public class LlmChatMessage(string content, LlmChatMessageAuthorRole role = LlmChatMessageAuthorRole.User)
{
    /// <summary>
    ///     Содержимое сообщения
    /// </summary>
    [JsonPropertyName("content")] public string Content = content;

    /// <summary>
    ///     Роль автора сообщения
    /// </summary>
    [JsonPropertyName("role")] public LlmChatMessageAuthorRole Role = role;
}