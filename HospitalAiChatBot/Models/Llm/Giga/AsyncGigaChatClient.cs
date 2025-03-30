using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace HospitalAiChatBot.Models.Llm.Giga;

/// <inheritdoc/>
/// <summary>
/// Ассинхронный клиент чата с GigaChat
/// </summary>
public class AsyncGigaChatClient(
    GigaChatClientConfiguration configuration,
    IEnumerable<GigaChatMessage>? startChatMessages = null)
    : AsyncHttpLlmChatClient<GigaChatClientConfiguration, GigaChatMessage>(configuration, startChatMessages)
{
    private string _accessToken = string.Empty;

    // ReSharper disable once InvalidXmlDocComment
    /// <summary>
    /// Загружает файл в хранилище GigaChat
    /// <remarks><see cref="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-file?ext=text">Документация</see></remarks>
    /// </summary>
    /// <param name="filePath">URI загружаемого файла</param>
    /// <param name="fileMimeType">MIME-тип загружаемого файла</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Идентификатор файла, который можно использовать при формировании сообщения в поле <see cref="GigaChatMessage.Attachments"/></returns>
    public async Task<Guid> UploadFileAsync(string filePath, string fileMimeType,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://gigachat.devices.sberbank.ru/api/v1/files");

        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Authorization", $"Bearer {_accessToken}");

        var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath, cancellationToken));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(fileMimeType);

        var content = new MultipartFormDataContent();
        content.Add(fileContent, "file", Path.GetFileName(filePath));
        content.Add(new StringContent("general"), "purpose");
        content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
        request.Content = content;

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        // TODO: проверка null
        // return (Guid)jsonFileUploadResponse["id"]!;
        return Guid.Empty;
    }

    // TODO: возвращать массив FileInfo, а не только их количество
    /// <summary>
    /// Получает количество загруженных ранее файлов с помощью метода <see cref="UploadFileAsync"/>
    /// </summary>
    /// <returns>Количество загруженных файлов</returns>
    public async Task<int> GetUploadedFilesCountAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        var httpFileUploadRequestMessage =
            new HttpRequestMessage(HttpMethod.Get, "https://gigachat.devices.sberbank.ru/api/v1/files");

        httpFileUploadRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpFileUploadRequestMessage.Headers.Add("Authorization", $"Bearer {_accessToken}");

        var httpFileUploadResponseMessage =
            await _httpClient.SendAsync(httpFileUploadRequestMessage, cancellationToken);
        httpFileUploadResponseMessage.EnsureSuccessStatusCode();

        var jsonFileUploadResponse =
            await httpFileUploadResponseMessage.Content.ReadFromJsonAsync<JsonObject>(cancellationToken);
        ArgumentNullException.ThrowIfNull(jsonFileUploadResponse, nameof(jsonFileUploadResponse));

        // TODO: проверка null
        var filesInfo = (JsonArray)jsonFileUploadResponse["data"]!;

        return filesInfo.Count;
    }

    /// <summary>
    /// Удаляет загруженный ранее файл с помощью метода <see cref="UploadFileAsync"/> из хранилища GigaChat
    /// </summary>
    /// <param name="uploadedFileId">Идентификатор удаляемого файла, полученный при загрузке файла</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Признак успешного удаления файла</returns>
    public async Task<bool> DeleteUploadedFileAsync(Guid uploadedFileId, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        var httpFileUploadRequestMessage =
            new HttpRequestMessage(HttpMethod.Post,
                $"https://gigachat.devices.sberbank.ru/api/v1/files/{uploadedFileId}/delete");

        httpFileUploadRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpFileUploadRequestMessage.Headers.Add("Authorization", $"Bearer {_accessToken}");

        var httpFileUploadResponseMessage =
            await _httpClient.SendAsync(httpFileUploadRequestMessage, cancellationToken);
        httpFileUploadResponseMessage.EnsureSuccessStatusCode();

        var jsonFileUploadResponse =
            await httpFileUploadResponseMessage.Content.ReadFromJsonAsync<JsonObject>(cancellationToken);
        ArgumentNullException.ThrowIfNull(jsonFileUploadResponse, nameof(jsonFileUploadResponse));

        // TODO: проверка null
        return (bool)jsonFileUploadResponse["deleted"]!;
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
    public override async Task<GigaChatMessage> SendMessages(GigaChatMessage? newMessage = null,
        bool isLlmAnswerMessageAddingToChatMessages = false, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (newMessage is not null)
            _chatMessages.Add(newMessage);

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
        var llmAnswerMessage = new GigaChatMessage(answerContent, LlmChatMessageAuthorRole.Assistant);

        if (isLlmAnswerMessageAddingToChatMessages)
            _chatMessages.Add(llmAnswerMessage);

        return llmAnswerMessage;
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
}