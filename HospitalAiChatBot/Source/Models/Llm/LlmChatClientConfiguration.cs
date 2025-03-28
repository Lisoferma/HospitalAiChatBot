using System.ComponentModel.DataAnnotations;

namespace HospitalAiChatBot.Source.Models.Llm;

/// <summary>
///     Общая конфигурация чта-клиента с LLM
/// </summary>
/// <param name="Temperature">
///     <para>Температура выборки.</para>
///     Чем выше значение, тем более случайным будет ответ модели
///     <remarks>Значение по умолчанию: 0.7</remarks>
/// </param>
/// <param name="TopP">
///     <para>Альтернатива <see cref="Temperature">температуре выборки.</see></para>
///     Задает вероятностную массу токенов, которые должна учитывать модель.
///     <remarks>Значение по умолчанию: 0.7</remarks>
/// </param>
/// <param name="RepetitionPenalty">
///     <para>
///         Величина, характеризующая повторение слов в ответе от LLM
///     </para>
///     Чем больше значение данной величины, тем больше модель в ответе будет стараться не повторять слова.
///     <remarks>Нейтральное значение - 1.0</remarks>
/// </param>
/// <param name="MaxTokens">
///     Максимальное количество токенов в ответе от LLM.
///     <remarks>Значение по умолчанию: 2048</remarks>
/// </param>
/// <param name="IsLlmAnswerStreaming">
///     Определяет, будет ли ответ от LLM в виде потока частей сообщения или в виде одного
///     сообщения.
///     <remarks>Значение по умолчанию: false</remarks>
/// </param>
public record LlmChatClientConfiguration(
    float Temperature = 0.7f,
    [Range(0, 1)] float TopP = 0.9f,
    float RepetitionPenalty = 1.0f,
    int MaxTokens = 2048,
    bool IsLlmAnswerStreaming = false);