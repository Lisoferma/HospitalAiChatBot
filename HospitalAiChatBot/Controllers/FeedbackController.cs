using Microsoft.AspNetCore.Mvc;

namespace OpenAPIServer.Controllers
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
