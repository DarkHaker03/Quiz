namespace Quiz.Models.DTOs
{
    /// <summary>
    /// DTO для отправки ответа на отдельный вопрос
    /// </summary>
    public class SubmitAnswerDto
    {
        /// <summary>
        /// Код доступа к тесту
        /// </summary>
        public string AccessCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Ответ на вопрос
        /// </summary>
        public QuestionAnswerDto Answer { get; set; } = new();
    }
} 