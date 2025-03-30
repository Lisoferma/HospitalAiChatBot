using HospitalAiChatBot.Models.Llm;
using HospitalAiChatBot.Models.Llm.Giga;

namespace HospitalAiChatBot.Tests;

[TestFixture]
public class AsyncGigaChatClientTests
{
    private static readonly string EnvironmentalClientId =
        Environment.GetEnvironmentVariable("GIGA_CHAT_CLIENT_ID") ?? "";

    private static readonly string EnvironmentalClientSecret =
        Environment.GetEnvironmentVariable("GIGA_CHAT_CLIENT_SECRET") ?? "";

    private static readonly GigaChatClientConfiguration Configuration = new(EnvironmentalClientId,
        EnvironmentalClientSecret,
        GigaChatApiScope.Personal,
        GigaChatApiModel.GigaChat);

    private static readonly AsyncGigaChatClient Client = new(Configuration);

    [Test]
    [Ignore("Потенциальное отсутствие ключей")]
    public async Task TestAuthTokenUpdateWithEnvironmentVariables()
    {
        await Client.UpdateAccessTokenAsync();
    }


    [Test]
    [Ignore("Потенциальное отсутствие ключей; Токены ограничены")]
    public async Task TestGetAnswerToTheMessageWithEnvironmentsVariables()
    {
        var message = new GigaChatMessage("Молчи", LlmChatMessageAuthorRole.System);
        var _ = await Client.SendMessages(message);
    }

    [TestCase(["Тестирование количества - раз", "Выявление значения - два"])]
    [TestCase("Тестирование количества - раз\nВыявление значения - два")]
    [Ignore("Всё уже протестировано - два сообщения меньше, чем одно, т.к. 1-ое ещё и с '\n' символом")]
    public async Task TestMessageUsedTokensCount(params string[] messagesContent)
    {
        var len = messagesContent.Length;
        var messages = new GigaChatMessage[len];
        for (var i = 0; i < len; i++)
            messages[i] = new GigaChatMessage(messagesContent[i], LlmChatMessageAuthorRole.User);
        Client.ChatMessages = messages;
        await Client.UpdateAccessTokenAsync();
        var answer = await Client.SendMessages();
        Console.WriteLine(answer);
    }
}