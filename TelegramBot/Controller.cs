using HospitalAiChatBot.Models;
using HospitalAiChatBot.Models.ScenarioDeterminant;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
        _strToScenario.Add("Записаться к врачу", RequestedScenario.MakeAppointment);
        _strToScenario.Add("Обратная связь", RequestedScenario.Feedback);
        _strToScenario.Add("Связаться с оператором", RequestedScenario.CommunicationWithOperator);
        _strToScenario.Add("Отложенный звонок", RequestedScenario.DefferedAnswer);

        _httpClient = new HttpClient();

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
            state.ImportantMessages = null;
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

    /// <summary>
    /// Возвращает контакты колл-центра
    /// </summary>
    /// <param name="msg">Собщение пользователя</param>
    /// <returns></returns>
    async Task OnCommunicationWithOperator(Message msg)
    {
        HttpResponseMessage contactsResp;

        contactsResp = await _httpClient.GetAsync(_webApiUrl + "/api/scrape/callcentercontacts");

        contactsResp.EnsureSuccessStatusCode();

        string contacts = await contactsResp.Content.ReadAsStringAsync();

        await _bot.SendMessage(msg.Chat, $"Контакты колл-центра:\n{contacts}");

        await OnStartMessage(msg);
    }

    /// <summary>
    /// Возврат информации о подготовке к исследованиям
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    async Task OnGetExaminationPrepareInfo(Message msg)
    {
        await _bot.SendMessage(msg.Chat, "Файл с информацией о подготовке к исследованиям:\n" +
            ChitgmaClinicScraper.PREPARING_PROCEDURE_URL);

        await OnStartMessage(msg);
    }

    /// <summary>
    /// Возврат информации о времени изготовления анализов
    /// </summary>
    /// <param name="msg">Сообщение пользователя</param>
    /// <returns></returns>
    async Task OnGetSamplesPreparingTimeInfo(Message msg)
    {
        await _bot.SendMessage(msg.Chat, """
        Сроки изготовления анализов согласно прайса:
          общеклинические в течении дня сдачи анализа,
          бактериологические исследования от 1 до 14 рабочих дней (зависит от исследования),
          молекулярная диагностика и иммунохроматографический анализ уточняются индивидуально        
        """);

        await OnStartMessage(msg);
    }

    /// <summary>
    /// Получение от специалиста времени работы врача или вообще обратная связь
    /// </summary>
    /// <param name="msg">Сообщение от пользователя</param>
    /// <returns></returns>
    async Task OnFeedbackOrGetDoctorWorkingTime(Message msg)
    {
        RequestedScenario scenario = _idToUserState[msg.From.Id].CurrentScenario;

        if (_idToUserState[msg.From.Id].State == States.ChoosingScenario)
        {
            string welcomeToScenarioMsg = scenario switch
            {
                RequestedScenario.GetDoctorWorkTime => "Время работы какого специалиста вы желаете узнать?",
                // RequestedScenario.Feedback
                _ => "Вы можете оставить отзыв или предложение, а также написать вопрос для специалиста",
            };

            await _bot.SendMessage(msg.Chat, welcomeToScenarioMsg);

            _idToUserState[msg.From.Id].State = States.InLongScenario;
        }

        else
        {
            MessageLink msgLink = new MessageLink(msg.Chat.Id, msg.Id);

            HttpClient httpClient = new HttpClient();

            if (_idToUserState[msg.From.Id].ImportantMessages == null)
            {
                _idToUserState[msg.From.Id].ImportantMessages = [msg];
            }

            Message userMsg = _idToUserState[msg.From.Id].ImportantMessages.First();

            Question question = new Question()
            {
                Contacts = JsonSerializer.Serialize(msgLink),
                Text = userMsg.Text,
                FromClientType = ClientType.TelegramBot
            };

            var response = await httpClient.PostAsync(_webApiUrl + "/api/specialistqa", JsonContent.Create(question));

            response.EnsureSuccessStatusCode();

            string internalMsgId = await response.Content.ReadAsStringAsync();

            string textToSpecialist = scenario switch
            {
                RequestedScenario.GetDoctorWorkTime => $"Время работы. ID обращения = {internalMsgId}",
                // RequestedScenario.Feedback
                _ => $"Обратная связь. ID обращения = {internalMsgId}"
            };

            await _bot.SendMessage(_specialistChatId, textToSpecialist);

            await _bot.ForwardMessage(_specialistChatId, userMsg.Chat, userMsg.Id);
            await _bot.SendMessage(userMsg.Chat, "Ваше сообщение передано специалисту");

            await OnStartMessage(msg);
        }
    }

    /// <summary>
    /// Обработка ответа специалиста на обратную связь
    /// </summary>
    /// <param name="msg">Сообщение специалиста, отвечающее на пересланное ему сообщение</param>
    /// <returns></returns>
    async Task OnFeedbackOrGetDoctorWorkingTimeAnswer(Message msg)
    {
        string regexAsStr = @"/a\s+(\S+)\s+((?:.|\n)+)";
        Regex regex = new Regex(regexAsStr, RegexOptions.Compiled);

        Match match = regex.Match(msg.Text);

        if (!match.Success)
        {
            await _bot.SendMessage(msg.Chat, $"Ваш запрос составлен неверно. Формат: {regexAsStr}");
            await OnStartMessage(msg);
        }

        else
        {
            HttpClient httpClient = new HttpClient();

            string userMessageId = match.Groups[1].Value;
            string textToUser = match.Groups[2].Value;

            var resp = await httpClient.GetAsync(_webApiUrl + $"/api/specialistqa/?questionId={userMessageId}");

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                await _bot.SendMessage(msg.Chat, "ID сообщения пользователя не был найден в словаре");
                return;
            }

            Question question = await resp.Content.ReadFromJsonAsync<Question>();

            MessageLink messageLink = JsonSerializer.Deserialize<MessageLink>(question.Contacts);

            await _bot.SendMessage(messageLink.ChatId, textToUser, replyParameters: messageLink.MsgId);
        }
    }

    /// <summary>
    /// Обратный звонок
    /// </summary>
    /// <param name="msg">Сообщение пользователя</param>
    /// <returns></returns>
    async Task OnPromiseToCall(Message msg)
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
            case RequestedScenario.MakeAppointment:
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

            case RequestedScenario.GetExaminationPrepareInfo:
                await OnGetExaminationPrepareInfo(msg);
                break;

            case RequestedScenario.GetSamplesPreparingTimeInfo:
                await OnGetSamplesPreparingTimeInfo(msg);
                break;

            case RequestedScenario.PromiseToCall:
                await OnPromiseToCall(msg);
                break;

            case RequestedScenario.Feedback:
            case RequestedScenario.GetDoctorWorkTime:
                try
                {
                    await OnFeedbackOrGetDoctorWorkingTime(msg);
                }
                catch (HttpRequestException ex)
                {
                    await HandleBotError(msg);

                    Console.WriteLine("Ошибка выполнения HTTP запроса: " + ex);
                }

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

    /// <summary>
    /// Обработка полученных сообщений
    /// </summary>
    /// <param name="msg">Сообщение пользователя</param>
    /// <param name="type"></param>
    /// <returns></returns>
    async Task OnMessage(Message msg, UpdateType type)
    {
        // Если в таблице не хранится состояния пользователя, тоже переходим к OnStartMessage
        if (msg.Text == "/start" || !_idToUserState.ContainsKey(msg.From.Id))
        {
            await OnStartMessage(msg);
        }

        else if (msg.Text?.StartsWith("/a ") ?? false && msg.Chat.Id == _specialistChatId)
        {
            await OnFeedbackOrGetDoctorWorkingTimeAnswer(msg);
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
    public List<Message>? ImportantMessages { get; set; }
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