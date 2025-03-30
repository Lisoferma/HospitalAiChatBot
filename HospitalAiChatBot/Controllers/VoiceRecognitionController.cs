using HospitalAiChatBot.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAiChatBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceRecognitionController : ControllerBase
    {
        private readonly IConfigurationRoot _config;
        private readonly string? _modelPath;
        private readonly ISpeachRecognizer _recognizer;


        public VoiceRecognitionController()
        {
            _config = new ConfigurationBuilder()
                .AddUserSecrets<VoiceRecognitionController>()
                .Build();

            _modelPath = _config["VoiceRecognition:ModelPath"];

            if (String.IsNullOrEmpty(_modelPath))
                throw new Exception(@"Для запуска распознавания речи, в secrets.json
                    необходимо задать ключ VoiceRecognition:ModelPath,
                    который содержит путь к модели нейросети.");

            Console.WriteLine(_modelPath);
            _recognizer = new VoskSpeechRecognizer(modelPath: _modelPath);
        }


        [HttpPost]
        public IActionResult PostVoice()
        {
            try
            {
                using Stream audioStream = Request.Body;
                if (audioStream.CanSeek && audioStream.Position == audioStream.Length)
                    return BadRequest("Empty stream.");

                string recognizedText = _recognizer.RecognizeOggStream(audioStream);

                return Ok(recognizedText);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}, StackTrace: {ex.StackTrace}");
            }
        }
    }
}
