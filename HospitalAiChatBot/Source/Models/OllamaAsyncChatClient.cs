using System.Text;
using System.Text.Json;

namespace HospitalAiChatbot.Source.Models
{
    public class OllamaAsyncChatClient : IDisposable
    {
        bool _disposed = false;
        readonly HttpClient _httpClient = new();
        public OllamaChatClientConfiguration Configuration;

        public void Dispose()
        {
            if (_disposed)
                return;

            _httpClient.Dispose();
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        /// <summary>
        /// Send promt to LLM with HTTP client
        /// </summary>
        /// <returns>Responce from LLM</returns>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="HttpRequestException"/>
        public async Task<string?> SendPromtAsync(string promt, string? overridingSuffix = null, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            overridingSuffix ??= Configuration.Suffix;

            var queryParameters = new
            {
                model = Configuration.ModelName,
                promt,
                suffix = overridingSuffix!,
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