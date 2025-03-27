namespace HospitalAiChatBot.Source.Models.Llm;

/// <summary>
///     Сообщение чата с LLM
/// </summary>
/// <param name="Content">Содержимое сообщения</param>
/// <param name="Role">Роль автора сообщения</param>
public record LlmChatMessage(string Content, LlmChatMessageAuthorRole Role = LlmChatMessageAuthorRole.User);