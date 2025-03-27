using System.Text.Json;
using Telegram.Bot;

const string TOKEN_FILENAME = "token.txt";
const string USERSTATE_FILENAME = "userstate.json";

string botToken;

try
{
    botToken = File.ReadAllText(TOKEN_FILENAME);
}

catch (FileNotFoundException)
{
    Console.WriteLine("Не найден файл с токеном token.txt");
    Console.WriteLine("Программа не может выполняться без этого файла");
    return;
}


Dictionary<long, UserState> idToUserState;

try
{
    // Чтение состояний пользователей из файла
    string readenJson = File.ReadAllText(USERSTATE_FILENAME);
    idToUserState = JsonSerializer.Deserialize<Dictionary<long, UserState>>(readenJson);
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
    idToUserState);

Console.WriteLine("Для завершения работы нажмите Enter");

Console.ReadLine();

// завершает работу бота
cts.Cancel();

// Сохранение состояний пользователей
string serializedJson = JsonSerializer.Serialize<Dictionary<long, UserState>>(idToUserState);
File.WriteAllText(USERSTATE_FILENAME, serializedJson);