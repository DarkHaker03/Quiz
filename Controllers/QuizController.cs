using Microsoft.AspNetCore.Mvc;
using Quiz.Models.DTOs;
using Quiz.Services;

namespace Quiz.Controllers
{
    /// <summary>
    /// Контроллер для управления тестами
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        /// <summary>
        /// Получить викторину по коду доступа
        /// </summary>
        /// <param name="accessCode">Уникальный код доступа к тесту</param>
        /// <returns>Информация о тесте с вопросами</returns>
        /// <response code="200">Возвращает информацию о тесте</response>
        /// <response code="404">Если тест не найден</response>
        [HttpGet("{accessCode}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(QuizDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<QuizDto>> GetQuizByAccessCode(string accessCode)
        {
            var quiz = await _quizService.GetQuizByAccessCodeAsync(accessCode);

            if (quiz is null)
            {
                return NotFound("Викторина не найдена");
            }

            return Ok(quiz);
        }

        /// <summary>
        /// Создать или обновить викторину
        /// </summary>
        /// <param name="createQuizDto">Данные для создания/обновления теста</param>
        /// <param name="quizId">Опциональный ID существующего теста для обновления</param>
        /// <returns>Созданный или обновленный тест с уникальным кодом доступа</returns>
        /// <response code="201">Возвращает созданный тест</response>
        /// <response code="200">Возвращает обновленный тест</response>
        /// <response code="400">Если данные некорректны</response>
        /// <response code="404">Если тест для обновления не найден</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(QuizDto))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(QuizDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<QuizDto>> CreateOrUpdateQuiz(CreateQuizDto createQuizDto, [FromQuery] int? quizId = null)
        {
            try
            {
                var quiz = await _quizService.CreateOrUpdateQuizAsync(createQuizDto, quizId);
                
                if (quizId.HasValue)
                {
                    // Если это обновление существующего теста
                    return Ok(quiz);
                }
                else
                {
                    // Если это создание нового теста
                    return CreatedAtAction(nameof(GetQuizByAccessCode), new { accessCode = quiz.AccessCode }, quiz);
                }
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Тест с ID {quizId} не найден");
            }
        }

        /// <summary>
        /// Сохранить ответ пользователя на отдельный вопрос
        /// </summary>
        /// <param name="submitAnswerDto">Данные с ответом на вопрос</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Статус сохранения</returns>
        /// <response code="200">Ответ успешно сохранен</response>
        /// <response code="400">Если данные некорректны</response>
        [HttpPost("answer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerDto submitAnswerDto, [FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Необходимо указать идентификатор пользователя (userId)");
            }

            // Сохраняем ответ
            await _quizService.SaveUserAnswerAsync(
                submitAnswerDto.AccessCode, 
                userId, 
                submitAnswerDto.Answer);

            return Ok(new { message = "Ответ сохранен" });
        }

        /// <summary>
        /// Получить сохраненные ответы пользователя
        /// </summary>
        /// <param name="accessCode">Код доступа к тесту</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Список сохраненных ответов</returns>
        /// <response code="200">Возвращает список ответов</response>
        /// <response code="400">Если идентификатор пользователя не указан</response>
        [HttpGet("{accessCode}/answers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<QuestionAnswerDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<QuestionAnswerDto>>> GetUserAnswers(string accessCode, [FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Необходимо указать идентификатор пользователя (userId)");
            }

            // Получаем ответы пользователя
            var answers = await _quizService.GetUserAnswersAsync(accessCode, userId);
            return Ok(answers);
        }

        /// <summary>
        /// Отправить все ответы и получить результаты
        /// </summary>
        /// <param name="accessCode">Код доступа к тесту</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Результаты прохождения теста</returns>
        /// <response code="200">Возвращает результаты теста</response>
        /// <response code="404">Если тест не найден</response>
        /// <response code="400">Если идентификатор пользователя не указан</response>
        [HttpPost("{accessCode}/submit")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(QuizResultDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<QuizResultDto>> SubmitQuiz(string accessCode, [FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Необходимо указать идентификатор пользователя (userId)");
            }

            try
            {
                // Получаем результаты
                var result = await _quizService.GetQuizResultsAsync(accessCode, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Викторина не найдена");
            }
        }
    }
} 