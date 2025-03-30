using HospitalAiChatBot.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Text.Json;

namespace HospitalAiChatbot.Controllers
{
    /// <summary>
    /// Контроллер для работы с обратной связью
    /// </summary>
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

        /// <summary>
        /// Записывает в БД вопрос, заданный пользователем
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> PostQuestionAsync([FromBody] Question question)
        {
            string questionId = await _repository.AddAsync(question);

            return questionId;
        }

        /// <summary>
        /// Возвращает вопрос, заданный пользователем, по его ID
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        [ProducesResponseType(200)]
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
