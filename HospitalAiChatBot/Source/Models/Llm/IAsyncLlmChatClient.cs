namespace HospitalAiChatBot.Source.Models.Llm;

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
    internal TConfiguration Configuration { get; set; }

    /// <summary>
    ///     Общий список сообщений чата
    /// </summary>
    internal IEnumerable<TMessage> ChatMessages { get; }

    /// <summary>
    ///     Список первоначальных сообщений чата, которые являются инициализирующими сообщениями чата.
    ///     <remarks>Данные сообщения остаются после <see cref="ResetChat">сброса чата</see></remarks>
    /// </summary>
    internal IEnumerable<TMessage> StartChatMessages { get; }

    /// <summary>
    ///     <para>Очищает весь список сообщений чата, инициализируя новый чат с LLM.</para>
    ///     <remarks>
    ///         При сбросе в списке сообщений остаются
    ///         <see cref="StartChatMessages">первоначальные сообщения</see>
    ///     </remarks>
    /// </summary>
    internal void ResetChat();

    /// <summary>
    ///     Посылает сообщение в чат и получает ответ от LLM  на сообщение.
    ///     <remarks>Ответ будет добавлен в конец списка сообщений чата.</remarks>
    /// </summary>
    /// <param name="message">Сообщение клиента</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Ответ от LLM</returns>
    internal Task<TMessage> SendMessage(TMessage message, CancellationToken cancellationToken = default);
}