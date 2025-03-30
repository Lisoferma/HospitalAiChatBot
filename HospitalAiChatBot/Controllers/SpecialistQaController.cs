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
            string questionId = await _repository.AddAsync(question);

            return questionId;
        }

        [HttpGet]
        public async Task<IActionResult> GetQuestionAsync(string questionId)
        {
            Question? question = await _repository.GetByIdAsync(questionId);

            if (question == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            return Ok(question);
        }
    }
}
