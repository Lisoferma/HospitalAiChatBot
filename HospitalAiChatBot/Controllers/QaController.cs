using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace HospitalAiChatbot.Controllers
{
    /// <summary>
    /// Контроллер для общих вопросов по работе организации, взаимодействующий с нейросетью
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class QaController : ControllerBase
    {
        /// <summary>
        /// Получение ответа на общий вопрос от нейросети
        /// </summary>
        /// <param name="question">Вопрос</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetAnswer([FromBody] string question)
        {
            string answer = $"Ответ на ваш вопрос \"{question}\":\nне реализовано";

            JsonArray links = new JsonArray();
            links.Add(new JsonObject { ["src"] = "не реализовано" });

            JsonObject response = new JsonObject
            {
                ["answer"] = answer,
                ["links"] = links
            };

            return Ok(response);
        }
    }
}
