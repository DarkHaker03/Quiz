using Quiz.Models;
using Quiz.Models.DTOs;

namespace Quiz.Services
{
    /// <summary>
    /// Сервис для работы с сессиями пользователей
    /// </summary>
    public interface IUserSessionService
    {
        /// <summary>
        /// Создать или получить сессию пользователя для теста
        /// </summary>
        Task<UserQuizSession> GetOrCreateSessionAsync(string accessCode, string userId);

        /// <summary>
        /// Сохранить ответ пользователя
        /// </summary>
        Task SaveAnswerAsync(string accessCode, string userId, QuestionAnswerDto answer);

        /// <summary>
        /// Получить все ответы пользователя по тесту
        /// </summary>
        Task<List<QuestionAnswerDto>> GetUserAnswersAsync(string accessCode, string userId);

        /// <summary>
        /// Завершить сессию и сохранить результаты
        /// </summary>
        Task CompleteSessionAsync(string accessCode, string userId, QuizResultDto results);

        /// <summary>
        /// Получить результаты прохождения теста
        /// </summary>
        Task<QuizResultDto?> GetSessionResultsAsync(string accessCode, string userId);

        /// <summary>
        /// Удалить сессию
        /// </summary>
        Task ClearSessionAsync(string accessCode, string userId);
    }
} 