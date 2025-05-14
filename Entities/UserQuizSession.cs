using Quiz.Models.DTOs;

namespace Quiz.Models
{
    /// <summary>
    /// Модель сессии пользователя для прохождения теста
    /// </summary>
    public class UserQuizSession
    {
        /// <summary>
        /// Уникальный идентификатор сессии
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Код доступа к тесту
        /// </summary>
        public string AccessCode { get; set; } = string.Empty;

        /// <summary>
        /// Время начала прохождения теста
        /// </summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Время последнего обновления
        /// </summary>
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Список ответов пользователя
        /// </summary>
        public List<QuestionAnswerDto> Answers { get; set; } = new();

        /// <summary>
        /// Завершено ли прохождение теста
        /// </summary>
        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// Результаты теста (если завершен)
        /// </summary>
        public QuizResultDto? Results { get; set; }
    }
} 