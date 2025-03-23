using System.Text;
using System.Text.Json;
using HospitalAiChatBot.Source.Models.LlmChatClient;

namespace HospitalAiChatbot.Source.Models.LlmChatClient
{
    /// <summary>
    /// Конфигурация чат клиента для модели развёрнутой с помощью <see cref="https://ollama.com/">Ollama</see> 
    /// </summary>
    /// <param name="ApiUri">URI API адрес развёрнутой LLM модели</param>
    /// <param name="ModelName">Имя модели</param>
    /// <param name="Suffix">Суффикс к запросу модели. <see cref="https://ollama.icu/api/#Generate_a_completion">Ollama API документация</see></param>
    /// <param name="IsStreamResponce">Получить ли ответ в виде потока или в виде одного сообщения</param>
    public record OllamaChatClientConfiguration(Uri ApiUri, string ModelName, bool IsStreamResponce = true);

    public class OllamaAsyncChatClient : IAsyncLlmChatClient
    {
        bool _disposed = false;
        readonly HttpClient _httpClient = new();
        public required OllamaChatClientConfiguration Configuration { get; set; }

        public void Dispose()
        {
            if (_disposed)
                return;

            _httpClient.Dispose();
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        /// <inheritdoc />
        /// <exception cref="HttpRequestException" />
        public async Task<string?> SendPromtAsync(string promt, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var queryParameters = new
            {
                model = Configuration.ModelName,
                promt,
                stream = Configuration.IsStreamResponce,
            };
            var queryJson = JsonSerializer.Serialize(queryParameters)!;
            var query = new StringContent(queryJson, Encoding.UTF8, "application/json");
            var responce = await _httpClient.PostAsync(Configuration.ApiUri, query, cancellationToken);
            responce.EnsureSuccessStatusCode();
            return await responce.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}