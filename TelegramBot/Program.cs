using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Telegram.Bot;

const string USERSTATE_FILENAME = "userstate.json";

IConfigurationRoot config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

string? botToken = config.GetValue<string>("botToken");
string? specialistChatIdStr = config["specialistChatId"];

if (botToken == null || specialistChatIdStr == null)
{
    Console.WriteLine("Для запуска бота в secrets.json необходимо задать ключи botToken и specialistChatId");
    return;
}

long specialistChatId = long.Parse(specialistChatIdStr);


Dictionary<long, UserState> idToUserState;

try
{
    // Чтение состояний пользователей из файла
    string readenJson = File.ReadAllText(USERSTATE_FILENAME);
    idToUserState = JsonSerializer.Deserialize<Dictionary<long, UserState>>(readenJson)
        ?? new Dictionary<long, UserState>();
}

catch (FileNotFoundException)
{
    Console.WriteLine("Файл с состояниями пользователей не найден");
    Console.WriteLine("Состояния пользователей обнуляются");
    idToUserState = new Dictionary<long, UserState>();
}


using var cts = new CancellationTokenSource();
Controller controller = new Controller(
    new TelegramBotClient(botToken, cancellationToken: cts.Token),
    idToUserState,
    specialistChatId);

Console.WriteLine("Для завершения работы нажмите Enter");

Console.ReadLine();

// завершает работу бота
cts.Cancel();

// Сохранение состояний пользователей
string serializedJson = JsonSerializer.Serialize<Dictionary<long, UserState>>(idToUserState);
File.WriteAllText(USERSTATE_FILENAME, serializedJson);