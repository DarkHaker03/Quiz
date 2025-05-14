namespace Quiz.Models.DTOs
{
    /// <summary>
    /// DTO для отправки ответов на викторину
    /// </summary>
    public class SubmitQuizDto
    {
        public string AccessCode { get; set; } = string.Empty;
        public List<QuestionAnswerDto> Answers { get; set; } = new();
    }
} 