using HospitalAIChatbot.Source.Models;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class Controller
{
    private TelegramBotClient bot;
    private Dictionary<long, UserState> idToUserState;
    //private IHospitalSiteScraper scraper;

    public Controller(TelegramBotClient aBot, Dictionary<long, UserState> idToUserState)
    {
        bot = aBot;

        bot.OnError += OnError;
        bot.OnMessage += OnMessage;

        this.idToUserState = idToUserState;

        //scraper = new RzdMedicineSiteScraper();
    }

    async Task OnStartMessage(Message msg)
    {
        long userId = msg.From.Id;

        string startMsg;

        if (idToUserState.ContainsKey(userId))
        {
            UserState state = idToUserState[userId];
            state.IsAtStart = true;
            startMsg = "Что-нибудь еще?";
        }

        else
        {
            startMsg = """
            Здравствуйте!
            Я могу записать вас к врачу, поделиться с вами информацией о поликлинике и т.п.
            Что вы хотите сделать?
            """;
            UserState state = new UserState();
            idToUserState.Add(userId, state);
        }


        Dictionary<Scenarios, String> scenarioToStr = new();

        scenarioToStr.Add(Scenarios.GetWorkTimeAndContacts, "Контакты колл-центра и часы работы");
        scenarioToStr.Add(Scenarios.GetDoctorWorkTime, "Часы работы специалиста");
        scenarioToStr.Add(Scenarios.GetExaminationPrepareInfo, "Подготовка к исследованию");
        scenarioToStr.Add(Scenarios.GetSamplesPreparingTimeInfo, "Время изготовления анализов");
        scenarioToStr.Add(Scenarios.MakeAppointment, "Записаться к специалисту");
        scenarioToStr.Add(Scenarios.Feedback, "Обратная связь");
        scenarioToStr.Add(Scenarios.CommunicationWithOperator, "Связаться с оператором");

        string[] buttonNames = scenarioToStr.Values.ToArray();

        string[][] customKeyboard = new string[scenarioToStr.Count][];

        for (int i = 0; i < buttonNames.Length; i++)
        {
            customKeyboard[i] = new string[] { buttonNames[i] };
        }

        await bot.SendMessage(msg.Chat, startMsg, replyMarkup: customKeyboard);
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
        if (msg.Text == "/start" || !idToUserState.ContainsKey(msg.From.Id))
        {
            await OnStartMessage(msg);
        }

        else if (msg.Text.ToLower().Contains("часы"))
        {
            string workingHours = "Пн-пт: с 08:00 до 20:00\nСб: с 08:00 до 14:00";
            await bot.SendMessage(msg.Chat, workingHours);
            await OnStartMessage(msg);
        }

        else if (msg.Text.ToLower().Contains("контакты"))
        {
            string contacts = "Баргузинская 49";
            await bot.SendMessage(msg.Chat, contacts);
            await OnStartMessage(msg);
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