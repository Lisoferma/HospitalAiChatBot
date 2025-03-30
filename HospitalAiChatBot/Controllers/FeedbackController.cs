using Microsoft.AspNetCore.Mvc;

namespace HospitalAiChatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        [HttpPost]
        public string PostFeedback([FromBody] string text)
        {
            return "Благодарим вас за отзыв!";
        }
    }
}
