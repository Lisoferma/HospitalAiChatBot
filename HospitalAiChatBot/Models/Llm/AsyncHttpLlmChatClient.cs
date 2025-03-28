namespace HospitalAiChatBot.Models.Llm;

/// <summary>
/// Ассинхронный клиент чата LLM с использованием <see cref="HttpClient"/>      
/// </summary>
/// <inheritdoc cref="AsyncLlmChatClient{TLlmChatClientConfiguration,TLlmChatMessage}"/>
public abstract class AsyncHttpLlmChatClient<TLlmChatClientConfiguration, TLlmChatMessage>(
    TLlmChatClientConfiguration configuration,
    IEnumerable<TLlmChatMessage>? startChatMessages = null)
    : AsyncLlmChatClient<TLlmChatClientConfiguration, TLlmChatMessage>(configuration, startChatMessages), IDisposable
    where TLlmChatClientConfiguration : LlmChatClientConfiguration
    where TLlmChatMessage : LlmChatMessage
{
    private protected readonly HttpClient _httpClient = new();
    private protected bool _isDisposed = false;

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _httpClient.Dispose();
        GC.SuppressFinalize(this);
        _isDisposed = true;
    }

    ~AsyncHttpLlmChatClient()
    {
        Dispose();
    }
}