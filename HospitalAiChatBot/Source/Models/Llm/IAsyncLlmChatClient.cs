using HospitalAiChatBot.Source.Models.Llm;

namespace HospitalAiChatbot.Source.Models.Llm;

/// <summary>
///     Ассинхронный клиент чата с LLM
/// </summary>
public interface IAsyncLlmChatClient
{
    /// <summary>
    ///     Конфигурация клиента чата
    /// </summary>
    LlmChatClientConfiguration Configuration { get; set; }

    /// <summary>
    ///     Общий список сообщений чата
    /// </summary>
    IEnumerable<LlmChatMessage> ChatMessages { get; }

    /// <summary>
    ///     Список первоначальных сообщений чата, которые являются инициализирующими сообщениями чата.
    ///     <remarks>Данные сообщения остаются после <see cref="IAsyncLlmChatClient.ResetChat">сброса чата</see></remarks>
    /// </summary>
    IEnumerable<LlmChatMessage> StartChatMessages { get; }

    /// <summary>
    ///     <para>Очищает весь список сообщений чата, инициализируя новый чат с LLM.</para>
    ///     <remarks>
    ///         При сбросе в списке сообщений остаются
    ///         <see cref="IAsyncLlmChatClient.StartChatMessages">первоначальные сообщения</see>
    ///     </remarks>
    /// </summary>
    void ResetChat();

    /// <summary>
    ///     Получает ответ от LLM  на сообщение клиента.
    ///     <remarks>Ответ будет добавлен в конец списка сообщений чата.</remarks>
    /// </summary>
    /// <param name="message">Сообщение клиента</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Ответ от LLM</returns>
    Task<string> GetAnswerToTheMessage(LlmChatMessage message, CancellationToken cancellationToken = default);
}