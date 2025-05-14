using System.ComponentModel.DataAnnotations;

namespace Quiz.Models
{
    /// <summary>
    /// Модель для представления варианта ответа
    /// </summary>
    public class Answer
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;

        [Required]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        public int Order { get; set; }
    }
} 