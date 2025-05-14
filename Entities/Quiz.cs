using System.ComponentModel.DataAnnotations;

namespace Quiz.Models
{
    /// <summary>
    /// Модель для представления викторины
    /// </summary>
    public class Quiz
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string AccessCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Question> Questions { get; set; } = new List<Question>();
    }
} 