using HospitalAiChatbot.Source.Models.Llm;

namespace HospitalAiChatBot.Source.Models.Llm.Giga;

/// <inheritdoc />
/// <summary>
///     Конфигурация чат-клиента GigaChat LLM
/// </summary>
/// <param name="ApiScope">Версия API чат-клиента</param>
public record GigaChatClientConfiguration(
    GigaChatApiScope ApiScope,
    float Temperature = 0.7f,
    float TopP = 0.9f,
    float RepetitionPenalty = 1,
    int MaxTokens = 2048,
    bool IsLlmAnswerStreaming = false)
    : LlmChatClientConfiguration(Temperature, TopP, RepetitionPenalty, MaxTokens, IsLlmAnswerStreaming);