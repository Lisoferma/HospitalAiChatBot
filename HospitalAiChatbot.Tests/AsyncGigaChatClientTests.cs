using HospitalAiChatBot.Source.Models.Llm;
using HospitalAiChatBot.Source.Models.Llm.Giga;

namespace HospitalAiChatbot.Tests;

[TestFixture]
public class AsyncGigaChatClientTests
{
    private static readonly string EnvironmentalClientId =
        Environment.GetEnvironmentVariable("GIGACHAT_CLIENT_ID") ??
        Environment.GetCommandLineArgs().GetValue(1) as string ?? "";

    private static readonly string EnvironmentalClientSecret =
        Environment.GetEnvironmentVariable("GIGA_CHAT_CLIENT_SECRET") ??
        Environment.GetCommandLineArgs().GetValue(2) as string ?? "";

    private static readonly GigaChatClientConfiguration Configuration = new(EnvironmentalClientId,
        EnvironmentalClientSecret,
        GigaChatApiScope.Personal,
        GigaChatApiModel.GigaChat);

    private static readonly AsyncGigaChatClient Client = new(Configuration);

    [Test]
    [Ignore("Не используется, из-за потенциального не наличия нужных значений среды")]
    public async Task TestAuthTokenUpdateWithEnvironmentVariables()
    {
        await Client.UpdateAccessTokenAsync();
    }


    [Test]
    [Ignore("Не используется, из-за потенциального не наличия нужных значений среды")]
    public async Task TestGetAnswerToTheMessageWithEnvironmentsVariables()
    {
        var message = new GigaChatMessage("Не отвечай", LlmChatMessageAuthorRole.Assistant);
        var _ = await Client.SendMessage(message);
    }
}