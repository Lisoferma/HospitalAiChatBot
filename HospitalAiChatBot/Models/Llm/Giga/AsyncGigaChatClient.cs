using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HospitalAiChatBot.Models.Llm.Giga;

public class AsyncGigaChatClient(
    GigaChatClientConfiguration configuration,
    IEnumerable<GigaChatMessage>? startChatMessages = null)
    : IAsyncLlmChatClient<GigaChatClientConfiguration, GigaChatMessage>, IDisposable
{
    private readonly List<GigaChatMessage> _chatMessages = [];
    private readonly HttpClient _httpClient = new();

    private readonly List<GigaChatMessage> _startChatMessages =
        startChatMessages is null ? [] : startChatMessages.ToList();

    private string _accessToken = string.Empty;
    private bool _isDisposed;

    public GigaChatClientConfiguration Configuration { get; set; } = configuration;
    public IEnumerable<GigaChatMessage> ChatMessages => _chatMessages;
    public IEnumerable<GigaChatMessage> StartChatMessages => _startChatMessages;

    public void ResetChat()
    {
        _chatMessages.Clear();
        _chatMessages.AddRange(_startChatMessages);
    }

    /// <inheritdoc />
    /// <exception cref="ObjectDisposedException">Если клиент был уже закрыт</exception>
    /// <exception cref="HttpRequestException">
    ///     Наиболее ожидаемые ошибки:
    ///     <para>400 - Некорректный формат запроса.</para>
    ///     <para>401 - Ошибка авторизации.</para>
    ///     <para>404 - Указан неверный идентификатор модели.</para>
    ///     <para>422 - Ошибка валидации параметров запроса. Проверьте названия полей и значения параметров.</para>
    ///     <para>429 - Слишком много запросов в единицу времени.</para>
    /// </exception>
    /// <exception cref="ArgumentNullException">В случае нулевого ответа от API</exception>
    public async Task<GigaChatMessage> SendMessage(GigaChatMessage message,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        _chatMessages.Add(message);

        HttpRequestMessage chatMessageAnswerRequest =
            new(HttpMethod.Post, "https://gigachat.devices.sberbank.ru/api/v1/chat/completions");
        chatMessageAnswerRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        chatMessageAnswerRequest.Headers.Add("Authorization", $"Bearer {_accessToken}");
        var requestContent = new
        {
            model = Configuration.Model.ToApiModelName(),
            messages = ChatMessages,
            temperature = Configuration.Temperature,
            top_p = Configuration.TopP,
            stream = Configuration.IsLlmAnswerStreaming,
            max_tokens = Configuration.MaxTokens,
            repetition_penalty = Configuration.RepetitionPenalty
        };
        chatMessageAnswerRequest.Content =
            new StringContent(JsonSerializer.Serialize(requestContent, JsonSerializerOptions.Web), Encoding.UTF8,
                new MediaTypeHeaderValue("application/json"));

        var rawChatResponse = await _httpClient.SendAsync(chatMessageAnswerRequest, cancellationToken);
        rawChatResponse.EnsureSuccessStatusCode();

        var chatResponseContent =
            await rawChatResponse.Content.ReadFromJsonAsync<JsonObject>(JsonSerializerOptions.Web, cancellationToken);
        ArgumentNullException.ThrowIfNull(chatResponseContent, nameof(chatResponseContent));

        // TODO: проверка на нулевое значение
        var answerContent = chatResponseContent["choices"]![0]![0]![0]!.ToString();

        return new GigaChatMessage(answerContent, LlmChatMessageAuthorRole.Assistant);
    }


    public void Dispose()
    {
        if (_isDisposed)
            return;

        _httpClient.Dispose();
        GC.SuppressFinalize(this);
        _isDisposed = true;
    }

    /// <summary>
    ///     Обновляет токен доступа к API.
    ///     <remarks>
    ///         <para>Токен доступа к API необходим для работы данного клиента.</para>
    ///         Токен доступа действителен в течение 30 минут
    ///     </remarks>
    /// </summary>
    /// <exception cref="ObjectDisposedException">Если клиент был уже закрыт</exception>
    /// <exception cref="HttpRequestException">Наиболее ожидаемые ошибки: 400, 401</exception>
    /// <exception cref="ArgumentNullException">В случае нулевого ответа от API</exception>
    public async Task UpdateAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        HttpRequestMessage authTokenRequest = new(HttpMethod.Post, "https://ngw.devices.sberbank.ru:9443/api/v2/oauth");
        var authStringBytes = Encoding.UTF8.GetBytes($"{Configuration.ClientId}:{Configuration.ClientSecret}");
        var authStringBase64 = Convert.ToBase64String(authStringBytes);

        authTokenRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        authTokenRequest.Headers.Add("RqUID", $"{Guid.NewGuid()}");
        authTokenRequest.Headers.Add("Authorization", $"Basic {authStringBase64}");
        authTokenRequest.Content = new StringContent($"scope={Configuration.ApiScope.ToApiScopeString()}");
        authTokenRequest.Content.Headers.ContentType =
            new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var response = await _httpClient.SendAsync(authTokenRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var authTokenRequestResponse = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken);
        ArgumentNullException.ThrowIfNull(authTokenRequestResponse, nameof(authTokenRequestResponse));

        // TODO: проверка на нулевое значение
        _accessToken = (string)authTokenRequestResponse["access_token"]!;
    }

    ~AsyncGigaChatClient()
    {
        Dispose();
    }
}