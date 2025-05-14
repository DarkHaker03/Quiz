using Microsoft.EntityFrameworkCore;
using Quiz.Data;
using Quiz.Models;
using Quiz.Models.DTOs;

namespace Quiz.Services
{
    /// <summary>
    /// Реализация сервиса для работы с тестами
    /// </summary>
    public class QuizService : IQuizService
    {
        private readonly QuizDbContext _context;
        private readonly IUserSessionService _sessionService;

        public QuizService(QuizDbContext context, IUserSessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        /// <inheritdoc />
        public async Task<QuizDto?> GetQuizByAccessCodeAsync(string accessCode)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.AccessCode == accessCode);

            if (quiz == null)
            {
                return null;
            }

            return new QuizDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                AccessCode = quiz.AccessCode,
                CreatedAt = quiz.CreatedAt,
                Questions = quiz.Questions
                    .OrderBy(q => q.Order)
                    .Select(q => new QuestionDto
                    {
                        Id = q.Id,
                        Text = q.Text,
                        Type = q.Type,
                        Order = q.Order,
                        Answers = q.Type == QuestionType.MultipleChoice ?
                            q.Answers.OrderBy(a => a.Order)
                                .Select(a => new AnswerDto
                                {
                                    Id = a.Id,
                                    Text = a.Text,
                                    Order = a.Order
                                }).ToList() : null
                    }).ToList()
            };
        }

        /// <inheritdoc />
        public async Task<QuizDto> CreateOrUpdateQuizAsync(CreateQuizDto createQuizDto, int? quizId = null)
        {
            Models.Quiz quiz;

            if (quizId.HasValue)
            {
                // Обновление существующего теста
                quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Id == quizId.Value);

                if (quiz == null)
                {
                    throw new KeyNotFoundException($"Тест с ID {quizId.Value} не найден");
                }

                // Обновляем основные поля теста
                quiz.Title = createQuizDto.Title;
                quiz.Description = createQuizDto.Description;

                // Получение всех существующих вопросов для последующей работы
                var existingQuestions = quiz.Questions.ToList();
                
                // Обновляем вопросы
                quiz.Questions.Clear();
                
                for (int i = 0; i < createQuizDto.Questions.Count; i++)
                {
                    var questionDto = createQuizDto.Questions[i];
                    Question question;
                    
                    // Поскольку CreateQuestionDto не имеет свойства Id, мы не можем определить
                    // какие вопросы обновлять. Создаем новые вопросы для каждого элемента в списке.
                    question = new Question
                    {
                        Text = questionDto.Text,
                        Type = questionDto.Type,
                        Order = questionDto.Order > 0 ? questionDto.Order : i + 1,
                        CorrectTextAnswer = questionDto.CorrectTextAnswer,
                        Answers = new List<Answer>()
                    };
                    
                    // Добавляем ответы для вопросов с множественным выбором
                    if (questionDto.Type == QuestionType.MultipleChoice && questionDto.Answers != null)
                    {
                        foreach (var answerDto in questionDto.Answers)
                        {
                            question.Answers.Add(new Answer
                            {
                                Text = answerDto.Text,
                                IsCorrect = answerDto.IsCorrect,
                                Order = answerDto.Order > 0 ? answerDto.Order : 0
                            });
                        }
                    }
                    
                    quiz.Questions.Add(question);
                }
                
                // Удаляем вопросы, которые были в базе данных
                foreach (var questionToRemove in existingQuestions)
                {
                    _context.Questions.Remove(questionToRemove);
                }
            }
            else
            {
                // Создание нового теста
                quiz = new Models.Quiz
                {
                    Title = createQuizDto.Title,
                    Description = createQuizDto.Description,
                    CreatedAt = DateTime.UtcNow,
                    AccessCode = GenerateAccessCode(),
                    Questions = new List<Question>()
                };
                
                // Добавляем вопросы
                for (int i = 0; i < createQuizDto.Questions.Count; i++)
                {
                    var questionDto = createQuizDto.Questions[i];
                    var question = new Question
                    {
                        Text = questionDto.Text,
                        Type = questionDto.Type,
                        Order = questionDto.Order > 0 ? questionDto.Order : i + 1,
                        CorrectTextAnswer = questionDto.CorrectTextAnswer,
                        Answers = new List<Answer>()
                    };
                    
                    // Добавляем ответы для вопросов с множественным выбором
                    if (questionDto.Type == QuestionType.MultipleChoice && questionDto.Answers != null)
                    {
                        for (int j = 0; j < questionDto.Answers.Count; j++)
                        {
                            var answerDto = questionDto.Answers[j];
                            question.Answers.Add(new Answer
                            {
                                Text = answerDto.Text,
                                IsCorrect = answerDto.IsCorrect,
                                Order = answerDto.Order > 0 ? answerDto.Order : j + 1
                            });
                        }
                    }
                    
                    quiz.Questions.Add(question);
                }

                _context.Quizzes.Add(quiz);
            }

            await _context.SaveChangesAsync();

            return new QuizDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                AccessCode = quiz.AccessCode,
                CreatedAt = quiz.CreatedAt,
                Questions = quiz.Questions
                    .OrderBy(q => q.Order)
                    .Select(q => new QuestionDto
                    {
                        Id = q.Id,
                        Text = q.Text,
                        Type = q.Type,
                        Order = q.Order,
                        Answers = q.Type == QuestionType.MultipleChoice ?
                            q.Answers.OrderBy(a => a.Order)
                                .Select(a => new AnswerDto
                                {
                                    Id = a.Id,
                                    Text = a.Text,
                                    Order = a.Order
                                }).ToList() : null
                    }).ToList()
            };
        }

        /// <inheritdoc />
        public async Task SaveUserAnswerAsync(string accessCode, string userId, QuestionAnswerDto answer)
        {
            await _sessionService.SaveAnswerAsync(accessCode, userId, answer);
        }

        /// <inheritdoc />
        public async Task<List<QuestionAnswerDto>> GetUserAnswersAsync(string accessCode, string userId)
        {
            return await _sessionService.GetUserAnswersAsync(accessCode, userId);
        }

        /// <inheritdoc />
        public async Task<QuizResultDto> GetQuizResultsAsync(string accessCode, string userId)
        {
            // Проверяем, есть ли уже рассчитанные результаты
            var existingResults = await _sessionService.GetSessionResultsAsync(accessCode, userId);
            if (existingResults is not null)
            {
                return existingResults;
            }

            // Получаем тест и ответы пользователя
            var userAnswers = await _sessionService.GetUserAnswersAsync(accessCode, userId);

            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.AccessCode == accessCode);

            if (quiz is null)
            {
                throw new KeyNotFoundException("Тест не найден");
            }

            // Рассчитываем результаты
            var result = CalculateResults(quiz, userAnswers);

            // Сохраняем результаты в БД
            await _sessionService.CompleteSessionAsync(accessCode, userId, result);

            return result;
        }

        /// <summary>
        /// Вычисление результатов теста
        /// </summary>
        private QuizResultDto CalculateResults(Models.Quiz quiz, List<QuestionAnswerDto> userAnswers)
        {
            var result = new QuizResultDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Questions = new List<QuestionResultDto>(),
                TotalQuestions = quiz.Questions.Count,
                CorrectAnswers = 0
            };

            foreach (var question in quiz.Questions)
            {
                var userAnswer = userAnswers.FirstOrDefault(a => a.QuestionId == question.Id);
                var questionResult = new QuestionResultDto
                {
                    Id = question.Id,
                    Text = question.Text,
                    Type = question.Type,
                    IsCorrect = false,
                    UserAnswer = userAnswer?.TextAnswer,
                    CorrectTextAnswer = question.Type == QuestionType.FreeText
                        ? question.CorrectTextAnswer
                        : null
                };

                if (question.Type == QuestionType.MultipleChoice)
                {
                    questionResult.Answers = question.Answers.Select(a => new AnswerResultDto
                    {
                        Id = a.Id,
                        Text = a.Text,
                        IsCorrect = a.IsCorrect,
                        IsSelected = userAnswer?.SelectedAnswerIds?.Contains(a.Id) ?? false
                    }).ToList();
                }

                // Проверяем правильность ответа
                if (userAnswer != null)
                {
                    if (question.Type == QuestionType.MultipleChoice)
                    {
                        var correctAnswerIds = question.Answers
                            .Where(a => a.IsCorrect)
                            .Select(a => a.Id)
                            .ToList();

                        var selectedAnswerIds = userAnswer.SelectedAnswerIds ?? new List<int>();

                        // Проверяем, что все правильные ответы выбраны и нет выбранных неправильных
                        questionResult.IsCorrect = correctAnswerIds.Count == selectedAnswerIds.Count &&
                                                  correctAnswerIds.All(selectedAnswerIds.Contains);
                    }
                    else if (question.Type == QuestionType.FreeText)
                    {
                        // Для текстовых вопросов сравниваем ответы без учета регистра
                        questionResult.IsCorrect = !string.IsNullOrEmpty(userAnswer.TextAnswer) &&
                                                 !string.IsNullOrEmpty(question.CorrectTextAnswer) &&
                                                 userAnswer.TextAnswer.Trim().Equals(
                                                     question.CorrectTextAnswer.Trim(),
                                                     StringComparison.OrdinalIgnoreCase);
                    }

                    if (questionResult.IsCorrect)
                    {
                        result.CorrectAnswers++;
                    }
                }

                result.Questions.Add(questionResult);
            }

            return result;
        }

        /// <summary>
        /// Генерация уникального кода доступа
        /// </summary>
        private string GenerateAccessCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string code;

            do
            {
                code = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            } while (_context.Quizzes.Any(q => q.AccessCode == code));

            return code;
        }
    }
} 