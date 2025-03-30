using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace HospitalAiChatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Проверка доступности сервера.
        /// </summary>
        /// <response code="200">Сервер доступен.</response>
        [HttpGet]
        [ProducesResponseType(200)]
        public IActionResult Get()
        {
            var response = new JsonObject
            {
                ["status"] = "ok",
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            return Ok(response);
        }
    }
}
