namespace HospitalAiChatBot.Models.Llm;

/// <summary>
///     Ассинхронный клиент чата с LLM
/// </summary>
public interface IAsyncLlmChatClient<TConfiguration, TMessage>
    where TConfiguration : LlmChatClientConfiguration
    where TMessage : LlmChatMessage
{
    /// <summary>
    ///     Конфигурация клиента чата
    /// </summary>
    TConfiguration Configuration { get; set; }

    /// <summary>
    ///     Общий список сообщений чата
    /// </summary>
    IEnumerable<TMessage> ChatMessages { get; }

    /// <summary>
    ///     Список первоначальных сообщений чата, которые являются инициализирующими сообщениями чата.
    ///     <remarks>Данные сообщения остаются после <see cref="ResetChat">сброса чата</see></remarks>
    /// </summary>
    IEnumerable<TMessage> StartChatMessages { get; }

    /// <summary>
    ///     <para>Очищает весь список сообщений чата, инициализируя новый чат с LLM.</para>
    ///     <remarks>
    ///         При сбросе в списке сообщений остаются
    ///         <see cref="StartChatMessages">первоначальные сообщения</see>
    ///     </remarks>
    /// </summary>
    void ResetChat();

    /// <summary>
    ///     Посылает сообщения чата LLM и получает ответ от LLM на сообщения.
    /// </summary>
    /// <param name="newMessage">Новое сообщение чата</param>
    /// <param name="isLlmAnswerMessageAddingToChatMessages">Определяет, будет ли добавлено ответное сообщение LLM в <see cref="ChatMessages"/></param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Ответ от LLM</returns>
    Task<TMessage> SendMessages(TMessage? newMessage = null, bool isLlmAnswerMessageAddingToChatMessages = false,
        CancellationToken cancellationToken = default);
}