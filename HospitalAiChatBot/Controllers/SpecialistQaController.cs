using HospitalAiChatBot.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HospitalAiChatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecialistQaController : ControllerBase
    {
        private const string CONNECTION_STRING = "mongodb://localhost:27017";
        private const string DATABASE_NAME = "chitgmaClinic";
        private const string COLLECTION_NAME = "questions";

        private readonly IRepository<Question> _repository;


        public SpecialistQaController()
        {
            _repository = new MongoRepository<Question>(CONNECTION_STRING, DATABASE_NAME, COLLECTION_NAME);
        }


        [HttpPost]
        public string PostQuestion([FromBody] string text)
        {
            Question? question = JsonSerializer.Deserialize<Question>(text);

            if (question != null)
                _repository.AddAsync(question);

            return "Благодарим вас за вопрос!";
        }
    }
}
