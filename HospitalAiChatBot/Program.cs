using HospitalAIChatbot.Source.Models;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

const string TOKEN_FILENAME = "token.txt";
const string USERSTATE_FILENAME = "userstate.json";

string BOT_TOKEN = File.ReadAllText(TOKEN_FILENAME);

Dictionary<long, UserState> idToUserState;

try
{
    // Чтение состояний пользователей из файла
    string readenJson = File.ReadAllText(USERSTATE_FILENAME);
    idToUserState = JsonSerializer.Deserialize<Dictionary<long, UserState>>(readenJson);
}

catch (FileNotFoundException)
{
    idToUserState = new Dictionary<long, UserState>();
}


using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(BOT_TOKEN);

bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;


Console.WriteLine("Для завершения работы нажмите Enter");

Console.ReadLine();
cts.Cancel(); // stop the bot

// Сохранение состояний пользователей
string serializedJson = JsonSerializer.Serialize<Dictionary<long, UserState>>(idToUserState);
File.WriteAllText(USERSTATE_FILENAME, serializedJson);




// Обработки разных ошибок
async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception);
}

// Обработка полученных сообщений
async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text == "/start")
    {
        await bot.SendMessage(msg.Chat, "Choose a response", replyMarkup: new string[][]
        {
            ["08:00", "08:30"],
            ["09:00", "09:30"],
            ["10:00", "10:30"]
        });

        //await bot.SendMessage(msg.Chat, "Выберите время для записи",
        //    replyMarkup: buttons.ToArray());
        //replyMarkup: new InlineKeyboardButton[] { "8:00", "9:00", "10:00", "11:00", "12:00", "13:00", "14:00" });
    }
}

// Обработка других обновлений
async Task OnUpdate(Update update)
{
    if (update is { CallbackQuery: { } query }) // non-null CallbackQuery
    {
        // Удаление кастомной клавиатуры, но не инлайновой:
        // await bot.SendMessage(chatId, "Removing keyboard", replyMarkup: new ReplyKeyboardRemove());
        await bot.AnswerCallbackQuery(query.Id, $"Вы записались на {query.Data}");
        await bot.SendMessage(
            query.Message!.Chat,
            $"Пользователь {query.From} записался на {query.Data}");
    }
}

class UserState
{
    public Scenarios CurrentScenario { get; set; }
    public List<string>? ImportantMessages { get; }
    public bool IsAtStart { get; set; }

    public UserState()
    {
        IsAtStart = true;
        ImportantMessages = null;
    }
}