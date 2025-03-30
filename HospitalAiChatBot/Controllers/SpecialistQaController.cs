using HospitalAiChatBot.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
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
        public async Task<string> PostQuestionAsync([FromBody] Question question)
        {
            question.Id = ObjectId.GenerateNewId().ToString();

            await _repository.AddAsync(question);

            IEnumerable<Question> result = await _repository.GetAllAsync();
            List<Question> questions = result.ToList();

            Console.WriteLine("----MongoDB----");
            foreach (var item in questions)
                Console.WriteLine($"Текст: {item.Text}, площадка: {item.FromClientType}, id: {item.Id}");

            return "Благодарим вас за вопрос!";
        }
    }
}
