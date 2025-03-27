using HospitalAiChatbot.Source.Models;
using Microsoft.AspNetCore.Http;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class Controller
{
    private TelegramBotClient _bot;
    private Dictionary<long, UserState> _idToUserState;
    private HttpClient _httpClient;
    private Dictionary<String, Scenarios> _strToScenario;
    private const string _webApiUrl = "http://localhost:5000";

    public Controller(TelegramBotClient aBot, Dictionary<long, UserState> idToUserState)
    {
        _bot = aBot;

        _bot.OnError += OnError;
        _bot.OnMessage += OnMessage;

        this._idToUserState = idToUserState;

        _strToScenario = new();

        _strToScenario.Add("Контакты колл-центра и часы работы", Scenarios.GetWorkTimeAndContacts);
        _strToScenario.Add("Часы работы специалиста", Scenarios.GetDoctorWorkTime);
        _strToScenario.Add("Подготовка к исследованию", Scenarios.GetExaminationPrepareInfo);
        _strToScenario.Add("Время изготовления анализов", Scenarios.GetSamplesPreparingTimeInfo);
        _strToScenario.Add("Записаться к специалисту", Scenarios.MakeAppointment);
        _strToScenario.Add("Обратная связь", Scenarios.Feedback);
        _strToScenario.Add("Связаться с оператором", Scenarios.CommunicationWithOperator);

        _httpClient = new HttpClient();
    }


    /// <summary>
    /// Обработчик для первого сообщения пользователя после /start
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
            state.IsAtStart = true;
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
        HttpResponseMessage contactsResp = await _httpClient.GetAsync(_webApiUrl + "/api/scrape/callcentercontacts");
        HttpResponseMessage workTimeResp = await _httpClient.GetAsync(_webApiUrl + "/api/scrape/openinghours");

        string contacts = await contactsResp.Content.ReadAsStringAsync();
        string workTime = await workTimeResp.Content.ReadAsStringAsync();

        await _bot.SendMessage(msg.Chat, $"""
            Контакты колл-центра:
            {contacts}

            Время работы:
            {workTime}
            """);
    }


    // Общие методы: обработчик ошибок и обработчик получений сообщений

    // Обработки разных ошибок
    async Task OnError(Exception exception, HandleErrorSource source)
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

        // !IsAtStart должен быть обработан здесь

        else if (_strToScenario.ContainsKey(msg.Text))
        {
            Scenarios scenario = _strToScenario[msg.Text];
            if (scenario == Scenarios.GetWorkTimeAndContacts)
            {
                await OnGetWorkTimeAndContacts(msg);
            }
        }
    }
}

/// <summary>
/// Состояние пользователя
/// </summary>
class UserState
{
    public Scenarios CurrentScenario { get; set; }
    public List<string>? ImportantMessages { get; }
    public bool IsAtStart { get; set; }

    public UserState()
    {
        IsAtStart = true;
    }
}