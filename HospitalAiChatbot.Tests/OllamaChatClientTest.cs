using System.Threading.Tasks;
using HospitalAiChatbot.Source.Models;
using HospitalAIChatbot.Source.Models;

namespace HospitalAiChatbot.Tests
{
    public class OllamaChatClientTest
    {
        [Test, Ignore("Not deploed LLM with ollama"), CancelAfter(60_000)]
        public async Task GetResponceTest()
        {
            Dictionary<Scenarios, string> scenariosDescriptions = new() {
                {Scenarios.GetWorkTimeAndContacts, "Getting hospital work time and contacts"},
                {Scenarios.GetDoctorWorkTime, "Get doctors (one of them or a group) work time"},
                {Scenarios.GetExaminationPrepareInfo, "Get info about examination prepare. Like stuff to be bringed, time of examination and so on"},
                {Scenarios.GetSamplesPreparingTimeInfo, "Get samples preparing time info"},
                {Scenarios.MakeAppointment, "Query to make a appointment"},
                {Scenarios.Feedback, "Some feedback of using application chatbot"},
                {Scenarios.DefferedAnswer, @"Some questions that can't be answered when client ask it,
                                            cause answer needs to be approved or clarified.
                                            For example: questions for doctors like 'If my dog bite me, which doctor I need to come?'"},
                {Scenarios.PromiseToCall, "Client asking hospital (call-center) to call client back"},
                {Scenarios.CommunicationWithOperator, "Client asking to chat with a call-center specialist"},
            };
            OllamaChatClientConfiguration config = new(
                new("http://localhost:11434/api/generate"),
                "genna3:1b",
                @$"Give your answer on RUSSIAN language only and keep answer in range of possible scenarios:
                [ {string.Join('\n', scenariosDescriptions.Select(pair => $"{pair.Key}: {pair.Value};"))} ]",
                false);

            var promt = @"You are a usual client of hospital. Write a text query for hospital";
            OllamaAsyncChatClient client = new()
            {
                Configuration = config
            };
            var responce = await client.SendPromtAsync(promt);
            Assert.That(string.IsNullOrWhiteSpace(responce));
        }
    }
}