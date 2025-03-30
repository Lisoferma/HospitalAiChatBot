using HospitalAiChatBot.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAiChatBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceRecognitionController : ControllerBase
    {
        const string MODEL_PATH = @"Resources\vosk-model-small-ru-0.22";
        private readonly ISpeachRecognizer _recognizer;


        public VoiceRecognitionController()
        {
            _recognizer = new VoskSpeechRecognizer(modelPath: MODEL_PATH);
        }


        [HttpPost("ogg")]
        public IActionResult PostVoice()
        {
            try
            {
                using Stream audioStream = Request.Body;
                if (audioStream == null || audioStream.Length == 0)
                    return BadRequest("Empty stream.");

                string recognizedText = _recognizer.RecognizeOggStream(audioStream);

                return Ok(recognizedText);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}
