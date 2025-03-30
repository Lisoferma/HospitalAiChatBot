using HospitalAiChatBot.Models.ScenarioDeterminant;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

class Controller
{
    private ChatId _specialistChatId;
    private TelegramBotClient _bot;
    private Dictionary<long, UserState> _idToUserState;
    private HttpClient _httpClient;
    private Dictionary<String, RequestedScenario> _strToScenario;
    private const string _webApiUrl = "http://localhost:5000";
    private Dictionary<int, MessageLink> _intrnlMsgIdToMsgLink;
    private int _intrnlMsgId;

    public Controller(TelegramBotClient aBot, Dictionary<long, UserState> idToUserState, long specialistChatId)
    {
        _bot = aBot;

        _bot.OnError += OnUncatchedError;
        _bot.OnMessage += OnMessage;

        this._idToUserState = idToUserState;

        _strToScenario = new();

        _strToScenario.Add("Контакты колл-центра и часы работы", RequestedScenario.GetWorkTimeAndContacts);
        _strToScenario.Add("Часы работы специалиста", RequestedScenario.GetDoctorWorkTime);
        _strToScenario.Add("Подготовка к исследованию", RequestedScenario.GetExaminationPrepareInfo);
        _strToScenario.Add("Время изготовления анализов", RequestedScenario.GetSamplesPreparingTimeInfo);
        _strToScenario.Add("Записаться к специалисту", RequestedScenario.MakeAppointment);
        _strToScenario.Add("Обратная связь", RequestedScenario.Feedback);
        _strToScenario.Add("Связаться с оператором", RequestedScenario.CommunicationWithOperator);
        _strToScenario.Add("Отложенный звонок", RequestedScenario.DefferedAnswer);

        _httpClient = new HttpClient();

        _intrnlMsgId = 0;

        _intrnlMsgIdToMsgLink = new Dictionary<int, MessageLink>();

        _specialistChatId = new ChatId(specialistChatId);
    }


    /// <summary>
    /// Обработчик для первого сообщения пользователя после /start
    /// Состояние пользователя устанавливается на выбор сценария
    /// </summary>
    /// <param name="msg">Сообщение пользователя</param>
    /// <returns></returns>
    async Task OnStartMessage(Message msg)
    {
        long userId = msg.From.Id;

        string startMsg;

        if (_idToUserState.ContainsKey(userId))
        {
            UserState state = _idToUserState[userId];
            state.State = States.ChoosingScenario;
            state.AtError = false;
            startMsg = "Что-нибудь еще?";
        }

        else
        {
            startMsg = "Здравствуйте! Вы обратились в чат-бот поликлиники. Мы готовы ответить на" +
                " ваши вопросы и помочь с записью к врачу, информацией о расписании работы" +
                " специалистов, а также предоставить данные о необходимых документах и услугах" +
                " нашей клиники. Пожалуйста, уточните ваш вопрос или просьбу.";
            UserState state = new UserState();
            _idToUserState.Add(userId, state);
        }

        // Добавление кнопок выполнения сценариев
        string[] buttonNames = _strToScenario.Keys.ToArray();

        string[][] customKeyboard = new string[_strToScenario.Count][];

        for (int i = 0; i < buttonNames.Length; i++)
        {
            customKeyboard[i] = new string[] { buttonNames[i] };
        }

        await _bot.SendMessage(msg.Chat, startMsg, replyMarkup: customKeyboard);
    }

    /// <summary>
    /// Отвечает пользователю контактами колл-центра и временем работы поликлиники
    /// </summary>
    /// <param name="msg">Сообщение пользователя</param>
    /// <returns></returns>
    async Task OnGetWorkTimeAndContacts(Message msg)
    {
        HttpResponseMessage contactsResp;
        HttpResponseMessage workTimeResp;

        contactsResp = await _httpClient.GetAsync(_webApiUrl + "/api/scrape/callcentercontacts");
        workTimeResp = await _httpClient.GetAsync(_webApiUrl + "/api/scrape/openinghours");

        contactsResp.EnsureSuccessStatusCode();
        workTimeResp.EnsureSuccessStatusCode();

        string contacts = await contactsResp.Content.ReadAsStringAsync();
        string workTime = await workTimeResp.Content.ReadAsStringAsync();

        await _bot.SendMessage(msg.Chat, $"""
            Контакты колл-центра:
            {contacts}

            Время работы:
            {workTime}
            """);

        await OnStartMessage(msg);
    }

    async Task OnCommunicationWithOperator(Message msg)
    {
        HttpResponseMessage contactsResp;

        contactsResp = await _httpClient.GetAsync(_webApiUrl + "/api/scrape/callcentercontacts");

        contactsResp.EnsureSuccessStatusCode();

        string contacts = await contactsResp.Content.ReadAsStringAsync();

        await _bot.SendMessage(msg.Chat, $"Контакты колл-центра:\n{contacts}");

        await OnStartMessage(msg);
    }

    async Task OnFeedback(Message msg)
    {
        if (_idToUserState[msg.From.Id].State == States.ChoosingScenario)
        {
            await _bot.SendMessage(msg.Chat, "Вы можете оставить отзыв или предложение, а также написать вопрос для специалиста");
            _idToUserState[msg.From.Id].State = States.InLongScenario;
        }

        else
        {
            _intrnlMsgIdToMsgLink[_intrnlMsgId] = new MessageLink(msg.Chat.Id, msg.Id);

            await _bot.SendMessage(_specialistChatId, $"Обратная связь. Id = {_intrnlMsgId}");

            _intrnlMsgId++;

            await _bot.ForwardMessage(_specialistChatId, msg.Chat, msg.Id);
            await _bot.SendMessage(msg.Chat, "Ваше сообщение передано специалисту");

            await OnStartMessage(msg);
        }
    }

    /// <summary>
    /// Обработка ответа специалиста на обратную связь
    /// </summary>
    /// <param name="msg">Сообщение специалиста, отвечающее на пересланное ему сообщение</param>
    /// <returns></returns>
    async Task OnFeedbackAnswer(Message msg)
    {
        Regex regex = new Regex(@"/a\s+(\d+)\s+(.*)", RegexOptions.Compiled);

        Match match = regex.Match(msg.Text);

        if (!match.Success)
        {
            await _bot.SendMessage(msg.Chat, $"Ваш запрос составлен неверно. Формат: {regex.ToString}");
            await OnStartMessage(msg);
        }

        else
        {
            int userMessageId = int.Parse(match.Groups[1].Value);
            string textToUser = match.Groups[2].Value;

            if (!_intrnlMsgIdToMsgLink.ContainsKey(userMessageId))
            {
                await _bot.SendMessage(msg.Chat, "ID сообщения пользователя не был найден в словаре");
                return;
            }

            MessageLink messageLink = _intrnlMsgIdToMsgLink[userMessageId];

            await _bot.SendMessage(messageLink.ChatId, textToUser, replyParameters: messageLink.MsgId);
        }
    }

    async Task OnDeferredAnswer(Message msg)
    {
        if (_idToUserState[msg.From.Id].State == States.ChoosingScenario)
        {
            await _bot.SendMessage(msg.Chat, "Вы можете передать свой номер для отложенного звонка. " +
                "Нажмите для этого на кнопку",
                replyMarkup: KeyboardButton.WithRequestContact("Поделиться номером телефона"));
            _idToUserState[msg.From.Id].State = States.InLongScenario;
        }

        else
        {
            if (msg.Contact == null)
            {
                await _bot.SendMessage(msg.Chat, "Ваш номер телефона не был передан");
                await OnStartMessage(msg);
                return;
            }

            string contactStr = $"{msg.Contact.FirstName} {msg.Contact.PhoneNumber}";

            await _bot.SendMessage(_specialistChatId, "Отложенный звонок");
            await _bot.ForwardMessage(_specialistChatId, msg.Chat.Id, msg.Id);
            await _bot.SendMessage(msg.Chat, "Ваш номер телефона был передан специалисту");

            await OnStartMessage(msg);
        }
    }

    /// <summary>
    /// Выполняет заданный сценарий
    /// </summary>
    async Task ExecuteScenario(Message msg, RequestedScenario scenario)
    {
        switch (scenario)
        {
            case RequestedScenario.GetWorkTimeAndContacts:
                try
                {
                    await OnGetWorkTimeAndContacts(msg);
                }

                catch (HttpRequestException ex)
                {
                    await HandleBotError(msg);

                    Console.WriteLine("Ошибка выполнения HTTP запроса: " + ex);
                }

                break;

            case RequestedScenario.CommunicationWithOperator:
                try
                {
                    await OnCommunicationWithOperator(msg);
                }

                catch (HttpRequestException ex)
                {
                    await HandleBotError(msg);

                    Console.WriteLine("Ошибка выполнения HTTP запроса: " + ex);
                }

                break;
            case RequestedScenario.DefferedAnswer:
                await OnDeferredAnswer(msg);
                break;

            case RequestedScenario.Feedback:
                await OnFeedback(msg);
                break;

            case RequestedScenario _:
                await _bot.SendMessage(msg.Chat, "Сценарий не реализован");
                break;
        }
    }

    /// <summary>
    /// Обработка ошибки на стороне бота
    /// </summary>
    /// <param name="msg">Сообщение от пользователя перед ошибкой</param>
    /// <returns></returns>
    async Task HandleBotError(Message msg)
    {
        _idToUserState[msg.From.Id].AtError = true;

        await _bot.SendMessage(
                msg.Chat,
                "Ошибка выполнения запроса. Хотели бы вы снова выполнить запрос (Да/Нет)?",
                replyMarkup: new string[] { "Да", "Нет" }
            );
    }

    /// <summary>
    /// Обработка сообщения после ошибки
    /// </summary>
    /// <param name="msg">Сообщение от пользователя после того как ему сообщили об ошибке</param>
    /// <returns></returns>
    async Task HandleMsgAfterBotError(Message msg)
    {
        string lower = msg.Text.ToLower();
        if (lower == "да")
        {
            long userid = msg.From.Id;

            _idToUserState[userid].AtError = false;
            RequestedScenario scenario = _idToUserState[userid].CurrentScenario;

            await ExecuteScenario(msg, scenario);
        }

        else if (lower == "нет")
        {
            await OnStartMessage(msg);
        }
    }



    // Общие методы: обработчик ошибок и обработчик получений сообщений

    // Обработки разных ошибок
    async Task OnUncatchedError(Exception exception, HandleErrorSource source)
    {
        Console.WriteLine(exception);
    }

    // Обработка полученных сообщений
    async Task OnMessage(Message msg, UpdateType type)
    {
        // Если в таблице не хранится состояния пользователя, тоже переходим к OnStartMessage
        if (msg.Text == "/start" || !_idToUserState.ContainsKey(msg.From.Id))
        {
            await OnStartMessage(msg);
        }

        else if (msg.Text?.StartsWith("/a ") ?? false && msg.Chat.Id == _specialistChatId)
        {
            await OnFeedbackAnswer(msg);
        }

        // State != ChoosingScenario должен быть обработан здесь

        else if (_idToUserState[msg.From.Id].State == States.InLongScenario)
        {
            await ExecuteScenario(msg, _idToUserState[msg.From.Id].CurrentScenario);
        }

        else if (_idToUserState[msg.From.Id].AtError)
        {
            await HandleMsgAfterBotError(msg);
        }

        // Пользователь в данном msg выбрал сценарий
        else if (_strToScenario.TryGetValue(msg.Text, out RequestedScenario scenario))
        {
            _idToUserState[msg.From.Id].CurrentScenario = scenario;

            await ExecuteScenario(msg, scenario);
        }
    }
}

/// <summary>
/// Состояние пользователя
/// </summary>
class UserState
{
    /// <summary>
    /// Выбранный пользователем сценарий
    /// </summary>
    public RequestedScenario CurrentScenario { get; set; }

    /// <summary>
    /// Сообщения пользователя, которые требуется хранить
    /// </summary>
    public List<string>? ImportantMessages { get; }
    /// <summary>
    /// Возникла ли ошибка
    /// </summary>
    public bool AtError { get; set; }
    /// <summary>
    /// Выполняется длинный сценарий или пользователь выбирает сценарий
    /// </summary>
    public States State { get; set; }

    public UserState()
    {
        State = States.ChoosingScenario;
        AtError = false;
        ImportantMessages = null;
    }
}

/// <summary>
/// Состояние пользователя: выбор сценарий или выполнение
/// сценария, для которого нужно отправить несколько сообщений
/// </summary>
enum States
{
    ChoosingScenario,
    InLongScenario
};

/// <summary>
/// Ссылка на сообщение
/// </summary>
/// <param name="ChatId">ID чата</param>
/// <param name="MsgId">ID сообщения</param>
record MessageLink(ChatId ChatId, int MsgId);