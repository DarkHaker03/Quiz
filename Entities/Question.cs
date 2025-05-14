using System.ComponentModel.DataAnnotations;

namespace Quiz.Models
{
    /// <summary>
    /// Тип вопроса
    /// </summary>
    public enum QuestionType
    {
        /// <summary>
        /// Вопрос с выбором ответа
        /// </summary>
        MultipleChoice,
        
        /// <summary>
        /// Вопрос с развернутым ответом
        /// </summary>
        FreeText
    }

    /// <summary>
    /// Модель для представления вопроса
    /// </summary>
    public class Question
    {
        public int Id { get; set; }

        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;

        [Required]
        public string Text { get; set; } = string.Empty;

        public QuestionType Type { get; set; }

        public int Order { get; set; }

        public List<Answer> Answers { get; set; } = new();

        /// <summary>
        /// Правильный ответ для вопросов с развернутым ответом
        /// </summary>
        public string? CorrectTextAnswer { get; set; }
    }
} 