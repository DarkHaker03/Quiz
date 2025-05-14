namespace Quiz.Models.DTOs
{
    /// <summary>
    /// DTO для создания нового варианта ответа
    /// </summary>
    public class CreateAnswerDto
    {
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int Order { get; set; }
    }

    /// <summary>
    /// DTO для получения данных о варианте ответа
    /// </summary>
    public class AnswerDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
    }

    /// <summary>
    /// DTO для получения результатов по варианту ответа
    /// </summary>
    public class AnswerResultDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public bool IsSelected { get; set; }
    }
} 