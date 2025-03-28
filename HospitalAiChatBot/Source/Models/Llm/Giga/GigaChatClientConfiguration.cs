using HospitalAiChatbot.Source.Models.Llm;

namespace HospitalAiChatBot.Source.Models.Llm.Giga;

/// <inheritdoc />
/// <summary>
///     Конфигурация чат-клиента GigaChat LLM
/// </summary>
/// <param name="ApiScope">Версия API чат-клиента</param>
/// <param name="ClientId">API ID клиента</param>
/// <param name="ClientSecret">API secret клиента</param>
/// <param name="Model">Используемая в чате модель</param>
public record GigaChatClientConfiguration(
    string ClientId,
    string ClientSecret,
    GigaChatApiScope ApiScope,
    GigaChatApiModel Model,
    float Temperature = 0.7f,
    float TopP = 0.9f,
    float RepetitionPenalty = 1,
    int MaxTokens = 2048,
    bool IsLlmAnswerStreaming = false)
    : LlmChatClientConfiguration(Temperature, TopP, RepetitionPenalty, MaxTokens, IsLlmAnswerStreaming);