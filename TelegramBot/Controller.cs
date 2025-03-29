using HospitalAiChatBot.Models.ScenarioDeterminant;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class Controller
{
    private TelegramBotClient _bot;
    private Dictionary<long, UserState> _idToUserState;
    private HttpClient _httpClient;
    private Dictionary<String, RequestedScenario> _strToScenario;
    private const string _webApiUrl = "http://localhost:5000";

    public Controller(TelegramBotClient aBot, Dictionary<long, UserState> idToUserState)
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

        _httpClient = new HttpClient();
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
                    return;
                }

                await OnStartMessage(msg);

                break;
            case RequestedScenario _:
                await _bot.SendMessage(msg.Chat, "Сценарий не реализован");
                break;
        }
    }

    async Task HandleBotError(Message msg)
    {
        _idToUserState[msg.From.Id].AtError = true;

        await _bot.SendMessage(
                msg.Chat,
                "Ошибка выполнения запроса. Хотели бы вы снова выполнить запрос (Да/Нет)?",
                replyMarkup: new string[] { "Да", "Нет" }
            );
    }

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

        // State != ChoosingScenario должен быть обработан здесь

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
    public bool AtError { get; set; }

    public States State { get; set; }

    public UserState()
    {
        State = States.ChoosingScenario;
        AtError = false;
        ImportantMessages = null;
    }
}

/// <summary>
/// Состояние пользователя: ошибка, выбор сценарий или выполнение
/// сценария, для которого нужно отправить несколько сообщений
/// </summary>
enum States
{
    ChoosingScenario,
    InLongScenario
};