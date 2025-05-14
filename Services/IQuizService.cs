using Quiz.Models;
using Quiz.Models.DTOs;

namespace Quiz.Services
{
    /// <summary>
    /// Сервис для работы с тестами
    /// </summary>
    public interface IQuizService
    {
        /// <summary>
        /// Получить тест по коду доступа
        /// </summary>
        Task<QuizDto?> GetQuizByAccessCodeAsync(string accessCode);

        /// <summary>
        /// Создать или обновить тест
        /// </summary>
        Task<QuizDto> CreateOrUpdateQuizAsync(CreateQuizDto createQuizDto, int? quizId = null);

        /// <summary>
        /// Сохранить ответ пользователя
        /// </summary>
        Task SaveUserAnswerAsync(string accessCode, string userId, QuestionAnswerDto answer);

        /// <summary>
        /// Получить сохраненные ответы пользователя
        /// </summary>
        Task<List<QuestionAnswerDto>> GetUserAnswersAsync(string accessCode, string userId);

        /// <summary>
        /// Проверить тест и получить результаты
        /// </summary>
        Task<QuizResultDto> GetQuizResultsAsync(string accessCode, string userId);
    }
} 