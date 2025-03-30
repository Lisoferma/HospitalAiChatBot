using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace HospitalAiChatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QaController : ControllerBase
    {
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
