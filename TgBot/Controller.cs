using HospitalAiChatbot.Source.Models;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

internal class Controller
{
    private const string _webApiUrl = "http://localhost:5000";
    private readonly TelegramBotClient _bot;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<long, UserState> _idToUserState;
    private readonly Dictionary<string, RequestedScenario> _strToScenario;

    public Controller(TelegramBotClient aBot, Dictionary<long, UserState> idToUserState)
    {
        _bot = aBot;

        _bot.OnError += OnError;
        _bot.OnMessage += OnMessage;

        _idToUserState = idToUserState;

        _strToScenario = new Dictionary<string, RequestedScenario>();

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
    ///     Обработчик для первого сообщения пользователя после /start
    /// </summary>
    /// <param name="msg">Сообщение пользователя</param>
    /// <returns></returns>
    private async Task OnStartMessage(Message msg)
    {
        var userId = msg.From.Id;

        string startMsg;

        if (_idToUserState.ContainsKey(userId))
        {
            var state = _idToUserState[userId];
            state.IsAtStart = true;
            startMsg = "Что-нибудь еще?";
        }

        else
        {
            startMsg = "Здравствуйте! Вы обратились в чат-бот поликлиники. Мы готовы ответить на" +
                       " ваши вопросы и помочь с записью к врачу, информацией о расписании работы" +
                       " специалистов, а также предоставить данные о необходимых документах и услугах" +
                       " нашей клиники. Пожалуйста, уточните ваш вопрос или просьбу.";
            var state = new UserState();
            _idToUserState.Add(userId, state);
        }

        // Добавление кнопок выполнения сценариев
        var buttonNames = _strToScenario.Keys.ToArray();

        var customKeyboard = new string[_strToScenario.Count][];

        for (var i = 0; i < buttonNames.Length; i++) customKeyboard[i] = new[] { buttonNames[i] };

        await _bot.SendMessage(msg.Chat, startMsg, replyMarkup: customKeyboard);
    }

    /// <summary>
    ///     Отвечает пользователю контактами колл-центра и временем работы поликлиники
    /// </summary>
    /// <param name="msg">Сообщение пользователя</param>
    /// <returns></returns>
    private async Task OnGetWorkTimeAndContacts(Message msg)
    {
        var contactsResp = await _httpClient.GetAsync(_webApiUrl + "/api/scrape/callcentercontacts");
        var workTimeResp = await _httpClient.GetAsync(_webApiUrl + "/api/scrape/openinghours");

        var contacts = await contactsResp.Content.ReadAsStringAsync();
        var workTime = await workTimeResp.Content.ReadAsStringAsync();

        await _bot.SendMessage(msg.Chat, $"""
                                          Контакты колл-центра:
                                          {contacts}

                                          Время работы:
                                          {workTime}
                                          """);
    }


    // Общие методы: обработчик ошибок и обработчик получений сообщений

    // Обработки разных ошибок
    private async Task OnError(Exception exception, HandleErrorSource source)
    {
        Console.WriteLine(exception);
    }

    // Обработка полученных сообщений
    private async Task OnMessage(Message msg, UpdateType type)
    {
        // Если в таблице не хранится состояния пользователя, тоже переходим к OnStartMessage
        if (msg.Text == "/start" || !_idToUserState.ContainsKey(msg.From.Id))
        {
            await OnStartMessage(msg);
        }

        // !IsAtStart должен быть обработан здесь

        else if (_strToScenario.ContainsKey(msg.Text))
        {
            var requestedScenario = _strToScenario[msg.Text];
            if (requestedScenario == RequestedScenario.GetWorkTimeAndContacts) await OnGetWorkTimeAndContacts(msg);
        }
    }
}

/// <summary>
///     Состояние пользователя
/// </summary>
internal class UserState
{
    public UserState()
    {
        IsAtStart = true;
    }

    public RequestedScenario CurrentRequestedScenario { get; set; }
    public List<string>? ImportantMessages { get; }
    public bool IsAtStart { get; set; }
}