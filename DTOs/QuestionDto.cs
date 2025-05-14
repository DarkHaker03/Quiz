using Quiz.Models;

namespace Quiz.Models.DTOs
{
    /// <summary>
    /// DTO для создания нового вопроса
    /// </summary>
    public class CreateQuestionDto
    {
        public string Text { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public int Order { get; set; }
        public List<CreateAnswerDto>? Answers { get; set; }
        public string? CorrectTextAnswer { get; set; }
    }

    /// <summary>
    /// DTO для получения данных о вопросе
    /// </summary>
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public int Order { get; set; }
        public List<AnswerDto>? Answers { get; set; }
    }

    /// <summary>
    /// DTO для получения результатов ответа на вопрос
    /// </summary>
    public class QuestionResultDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public string? UserAnswer { get; set; }
        public List<AnswerResultDto>? Answers { get; set; }
        public string? CorrectTextAnswer { get; set; }
        public bool IsCorrect { get; set; }
    }

    /// <summary>
    /// DTO для ответа на вопрос при отправке результатов
    /// </summary>
    public class QuestionAnswerDto
    {
        public int QuestionId { get; set; }
        public List<int>? SelectedAnswerIds { get; set; }
        public string? TextAnswer { get; set; }
    }
} 