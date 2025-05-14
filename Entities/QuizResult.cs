using System.ComponentModel.DataAnnotations;

namespace Quiz.Models
{
    /// <summary>
    /// Модель для хранения результатов прохождения теста
    /// </summary>
    public class QuizResult
    {
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Связь с тестом
        /// </summary>
        public int QuizId { get; set; }
        public Models.Quiz Quiz { get; set; } = null!;

        /// <summary>
        /// Общее количество вопросов
        /// </summary>
        public int TotalQuestions { get; set; }

        /// <summary>
        /// Количество правильных ответов
        /// </summary>
        public int CorrectAnswers { get; set; }

        /// <summary>
        /// Дата и время прохождения теста
        /// </summary>
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Процент правильных ответов
        /// </summary>
        public double ScorePercentage => TotalQuestions > 0 
            ? Math.Round((double)CorrectAnswers / TotalQuestions * 100, 2) 
            : 0;
    }
} 