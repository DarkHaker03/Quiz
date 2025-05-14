using System.ComponentModel.DataAnnotations;

namespace Quiz.Models
{
    /// <summary>
    /// Модель для хранения ответов пользователя в базе данных
    /// </summary>
    public class UserAnswer
    {
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Связь с вопросом
        /// </summary>
        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;

        /// <summary>
        /// Связь с тестом (для оптимизации запросов)
        /// </summary>
        public int QuizId { get; set; }
        public Models.Quiz Quiz { get; set; } = null!;

        /// <summary>
        /// Выбранные ответы (для вопросов с выбором)
        /// Хранится как строка с разделителями, например "1,2,3"
        /// </summary>
        public string? SelectedAnswerIds { get; set; }

        /// <summary>
        /// Текстовый ответ (для вопросов с развернутым ответом)
        /// </summary>
        public string? TextAnswer { get; set; }

        /// <summary>
        /// Время создания ответа
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Время последнего обновления ответа
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Правильность ответа (вычисляется при проверке)
        /// </summary>
        public bool? IsCorrect { get; set; }

        /// <summary>
        /// Получить список идентификаторов выбранных ответов
        /// </summary>
        public List<int> GetSelectedAnswerIdsList()
        {
            if (string.IsNullOrEmpty(SelectedAnswerIds))
                return new List<int>();

            return SelectedAnswerIds.Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(int.Parse)
                .ToList();
        }

        /// <summary>
        /// Установить список идентификаторов выбранных ответов
        /// </summary>
        public void SetSelectedAnswerIds(List<int> ids)
        {
            SelectedAnswerIds = ids.Count > 0 ? string.Join(",", ids) : null;
        }
    }
} 