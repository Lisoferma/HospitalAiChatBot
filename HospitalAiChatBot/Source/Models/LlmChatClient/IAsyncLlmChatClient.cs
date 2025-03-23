namespace HospitalAiChatBot.Source.Models.LlmChatClient
{
    public interface IAsyncLlmChatClient : IDisposable
    {
        /// <summary>
        /// Отправляет сообщение для LLM
        /// </summary>
        /// <returns>Ответ от LLM</returns>
        /// <exception cref="ObjectDisposedException"/>
        Task<string?> SendPromtAsync(string promt, CancellationToken cancellationToken = default);
    }
}