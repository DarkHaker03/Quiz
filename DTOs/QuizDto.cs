namespace Quiz.Models.DTOs
{
    /// <summary>
    /// DTO для создания новой викторины
    /// </summary>
    public class CreateQuizDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<CreateQuestionDto> Questions { get; set; } = new();
    }

    /// <summary>
    /// DTO для получения данных о викторине
    /// </summary>
    public class QuizDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string AccessCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
    }

    /// <summary>
    /// DTO для получения итогов викторины
    /// </summary>
    public class QuizResultDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<QuestionResultDto> Questions { get; set; } = new();
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
    }
} 