using Microsoft.AspNetCore.Mvc;

namespace HospitalAiChatbot.Controllers
{
    /// <summary>
    /// Контроллер для отзывов
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        /// <summary>
        /// Оставить отзыв
        /// </summary>
        /// <param name="text">Текст отзыва</param>
        /// <returns></returns>
        [HttpPost]
        public string PostFeedback([FromBody] string text)
        {
            return "Благодарим вас за отзыв!";
        }
    }
}
