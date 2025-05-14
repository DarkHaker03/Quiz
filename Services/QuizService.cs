using Microsoft.EntityFrameworkCore;
using Quiz.Data;
using Quiz.Models;
using Quiz.Models.DTOs;

namespace Quiz.Services
{
    /// <summary>
    /// Сервис для работы с тестами
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
        public async Task<QuizDto> CreateQuizAsync(CreateQuizDto createQuizDto)
        {
            // Генерация уникального кода доступа
            string accessCode = await GenerateUniqueAccessCode();

            var quiz = new Models.Quiz
            {
                Title = createQuizDto.Title,
                Description = createQuizDto.Description,
                AccessCode = accessCode,
                CreatedAt = DateTime.UtcNow,
                Questions = createQuizDto.Questions.Select((q, index) => new Question
                {
                    Text = q.Text,
                    Type = q.Type,
                    Order = q.Order > 0 ? q.Order : index + 1,
                    CorrectTextAnswer = q.CorrectTextAnswer,
                    Answers = q.Type == QuestionType.MultipleChoice && q.Answers != null
                        ? q.Answers.Select((a, i) => new Answer
                        {
                            Text = a.Text,
                            IsCorrect = a.IsCorrect,
                            Order = a.Order > 0 ? a.Order : i + 1
                        }).ToList()
                        : new List<Answer>()
                }).ToList()
            };

            _context.Quizzes.Add(quiz);
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
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.AccessCode == accessCode);

            if (quiz is null)
            {
                throw new KeyNotFoundException("Тест не найден");
            }

            var userAnswers = await _sessionService.GetUserAnswersAsync(accessCode, userId);

            // Формируем результаты
            var result = new QuizResultDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                TotalQuestions = quiz.Questions.Count,
                CorrectAnswers = 0,
                Questions = new List<QuestionResultDto>()
            };

            foreach (var question in quiz.Questions.OrderBy(q => q.Order))
            {
                var userAnswer = userAnswers.FirstOrDefault(a => a.QuestionId == question.Id);
                bool isCorrect = false;
                string? userAnswerText = null;

                if (question.Type == QuestionType.MultipleChoice)
                {
                    var selectedAnswers = userAnswer?.SelectedAnswerIds ?? new List<int>();
                    var correctAnswers = question.Answers.Where(a => a.IsCorrect).Select(a => a.Id).ToList();

                    // Проверка правильности ответов на вопрос с выбором
                    isCorrect = selectedAnswers.Count > 0 &&
                                correctAnswers.Count == selectedAnswers.Count &&
                                correctAnswers.All(selectedAnswers.Contains);

                    var answerResults = question.Answers.OrderBy(a => a.Order).Select(a => new AnswerResultDto
                    {
                        Id = a.Id,
                        Text = a.Text,
                        IsCorrect = a.IsCorrect,
                        IsSelected = selectedAnswers.Contains(a.Id)
                    }).ToList();

                    result.Questions.Add(new QuestionResultDto
                    {
                        Id = question.Id,
                        Text = question.Text,
                        Type = question.Type,
                        Answers = answerResults,
                        IsCorrect = isCorrect
                    });
                }
                else // FreeText
                {
                    userAnswerText = userAnswer?.TextAnswer;

                    // Для текстового вопроса проверяем совпадение с правильным ответом
                    isCorrect = !string.IsNullOrEmpty(userAnswerText) &&
                                !string.IsNullOrEmpty(question.CorrectTextAnswer) &&
                                userAnswerText.Trim().Equals(question.CorrectTextAnswer.Trim(),
                                    StringComparison.OrdinalIgnoreCase);

                    result.Questions.Add(new QuestionResultDto
                    {
                        Id = question.Id,
                        Text = question.Text,
                        Type = question.Type,
                        UserAnswer = userAnswerText,
                        CorrectTextAnswer = question.CorrectTextAnswer,
                        IsCorrect = isCorrect
                    });
                }

                if (isCorrect)
                {
                    result.CorrectAnswers++;
                }
            }

            // Сохраняем результаты
            await _sessionService.CompleteSessionAsync(accessCode, userId, result);

            return result;
        }

        /// <summary>
        /// Генерирует уникальный код доступа для викторины
        /// </summary>
        private async Task<string> GenerateUniqueAccessCode()
        {
            // Случайный код из 8 символов (буквы и цифры)
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string code;
            bool isUnique = false;

            // Проверяем на уникальность
            do
            {
                code = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                isUnique = !await _context.Quizzes.AnyAsync(q => q.AccessCode == code);
            } while (!isUnique);

            return code;
        }
    }
} 