// ReSharper disable InconsistentNaming

namespace HospitalAiChatBot.Models.Llm;

/// <summary>
/// Абстрактный клиент чата с LLM.
///  <remarks>Хранит сообщения списков, общий и первоначальный, в двух списках типа <see cref="List{T}"/></remarks>
/// </summary>
public abstract class
    AsyncLlmChatClient<TLlmChatClientConfiguration, TLlmChatMessage> : IAsyncLlmChatClient<TLlmChatClientConfiguration,
    TLlmChatMessage>
    where TLlmChatClientConfiguration : LlmChatClientConfiguration
    where TLlmChatMessage : LlmChatMessage
{
    protected List<TLlmChatMessage> _chatMessages;
    private readonly List<TLlmChatMessage> _startChatMessages;
    public TLlmChatClientConfiguration Configuration { get; set; }

    public virtual IEnumerable<TLlmChatMessage> ChatMessages
    {
        get => _chatMessages;
        set => _chatMessages = value.ToList();
    }

    public virtual IEnumerable<TLlmChatMessage> StartChatMessages => _startChatMessages;

    /// <summary>Конструктор с конфигурацией и начальными сообщениями чата</summary>
    /// <param name="configuration">Конфигурация клиента</param>
    /// <param name="startChatMessages">Начальные сообщения чата</param>
    protected AsyncLlmChatClient(TLlmChatClientConfiguration configuration,
        IEnumerable<TLlmChatMessage>? startChatMessages = null)
    {
        Configuration = configuration;
        _chatMessages = _startChatMessages = startChatMessages is null ? [] : startChatMessages.ToList();
    }

    public virtual void ResetChat()
    {
        _chatMessages.Clear();
        _chatMessages.AddRange(_startChatMessages);
    }

    public abstract Task<TLlmChatMessage> SendMessages(TLlmChatMessage? newMessage = null,
        bool isLlmAnswerMessageAddingToChatMessages = false, CancellationToken cancellationToken = default);
}