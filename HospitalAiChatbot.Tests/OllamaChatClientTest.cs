using HospitalAiChatbot.Source.Models;

namespace HospitalAiChatbot.Tests
{
    public class OllamaChatClientTest
    {
        [TestCase("gemma3:4b")]
        public async Task GetResponceTest(string modelName)
        {
            Dictionary<Scenarios, string> scenariosDescriptions = new() {
                {Scenarios.GetWorkTimeAndContacts, "Getting hospital work time and contacts"},
                {Scenarios.GetDoctorWorkTime, "Get doctors (one of them or a group) work time"},
                {Scenarios.GetExaminationPrepareInfo, "Get info about examination prepare. Like stuff to be bringed, time of examination and so on"},
                {Scenarios.GetSamplesPreparingTimeInfo, "Get samples preparing time info"},
                {Scenarios.MakeAppointment, "Query to make a appointment"},
                {Scenarios.Feedback, "Some feedback of using application chatbot"},
                {Scenarios.DefferedAnswer, "Some questions that can't be answered when client ask it,"+
                                            "cause answer needs to be approved or clarified."+
                                            "For example: questions for doctors like 'If my dog bite me, which doctor I need to come?'"},
                {Scenarios.PromiseToCall, "Client asking hospital (call-center) to call client back"},
                {Scenarios.CommunicationWithOperator, "Client asking to chat with a call-center specialist"},
            };
            OllamaChatClientConfiguration config = new(
                new("http://localhost:11434/api/generate"),
                modelName,
                false);

            var promt = "You are a default citizen and a client of hospital. Generate a text query for hospital" +
            "Generate answer on RUSSIAN ONLY. Don't reply as a hospital, only generate one query/ask as a client." +
            "Keep answer in ONLY ONE of range of possible scenarios" +
            $"( {string.Join(' ', scenariosDescriptions.Select(pair => $"{pair.Key}: {pair.Value};"))} )";
            OllamaAsyncChatClient client = new()
            {
                Configuration = config
            };
            _ = await client.SendPromtAsync(promt);
        }
    }
}