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
        /// Send promt to LLM
        /// </summary>
        /// <returns>Responce from LLM</returns>
        /// <exception cref="ObjectDisposedException"/>
        public async Task<string?> SendPromtAsync(string promt, string? overridingSuffix = null, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _httpClient.BaseAddress = Configuration.ApiUri;
            overridingSuffix ??= Configuration.Suffix;
            var query = $"{{ 'model': '{Configuration.ModelName}'," +
                        $"'promt': '{promt}', 'suffix': '{overridingSuffix}'," +
                        $"'stream': {Configuration.IsStreamResponce} }}";
            var responce = await _httpClient.PostAsync(query, null, cancellationToken);
            string responceContent = await responce.Content.ReadAsStringAsync(cancellationToken);

            return responceContent;
        }
    }
}