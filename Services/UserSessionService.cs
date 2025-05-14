using Microsoft.EntityFrameworkCore;
using Quiz.Data;
using Quiz.Models;
using Quiz.Models.DTOs;

namespace Quiz.Services
{
    /// <summary>
    /// Реализация сервиса для управления сессиями пользователей с использованием базы данных
    /// </summary>
    public class UserSessionService : IUserSessionService
    {
        private readonly QuizDbContext _context;

        public UserSessionService(QuizDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<UserQuizSession> GetOrCreateSessionAsync(string accessCode, string userId)
        {
            // Получаем тест по коду доступа
            var quiz = await _context.Quizzes
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.AccessCode == accessCode);

            if (quiz is null)
            {
                throw new KeyNotFoundException($"Тест с кодом доступа {accessCode} не найден");
            }

            // Получаем существующие ответы пользователя
            var userAnswers = await _context.UserAnswers
                .Where(ua => ua.Quiz.AccessCode == accessCode && ua.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            // Проверяем, есть ли результат прохождения
            var quizResult = await _context.QuizResults
                .AsNoTracking()
                .FirstOrDefaultAsync(qr => qr.Quiz.AccessCode == accessCode && qr.UserId == userId);

            // Создаем объект сессии на основе данных из БД
            var session = new UserQuizSession
            {
                UserId = userId,
                AccessCode = accessCode,
                StartedAt = userAnswers.Any() ? userAnswers.Min(ua => ua.CreatedAt) : DateTime.UtcNow,
                LastUpdatedAt = userAnswers.Any() ? userAnswers.Max(ua => ua.UpdatedAt) : DateTime.UtcNow,
                IsCompleted = quizResult != null,
                Answers = userAnswers.Select(ua => new QuestionAnswerDto
                {
                    QuestionId = ua.QuestionId,
                    SelectedAnswerIds = ua.GetSelectedAnswerIdsList(),
                    TextAnswer = ua.TextAnswer
                }).ToList()
            };

            // Если есть результат, заполняем его
            if (quizResult != null)
            {
                session.Results = new QuizResultDto
                {
                    Id = quiz.Id,
                    Title = quiz.Title,
                    TotalQuestions = quizResult.TotalQuestions,
                    CorrectAnswers = quizResult.CorrectAnswers,
                    Questions = await GetQuestionResultsFromUserAnswers(userAnswers)
                };
            }

            return session;
        }

        /// <inheritdoc />
        public async Task SaveAnswerAsync(string accessCode, string userId, QuestionAnswerDto answer)
        {
            // Получаем тест и вопрос
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.AccessCode == accessCode);

            if (quiz == null)
            {
                throw new KeyNotFoundException($"Тест с кодом доступа {accessCode} не найден");
            }

            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == answer.QuestionId && q.QuizId == quiz.Id);

            if (question == null)
            {
                throw new KeyNotFoundException($"Вопрос с ID {answer.QuestionId} не найден в тесте {accessCode}");
            }

            // Ищем существующий ответ
            var existingAnswer = await _context.UserAnswers
                .FirstOrDefaultAsync(ua => ua.QuizId == quiz.Id && ua.QuestionId == answer.QuestionId && ua.UserId == userId);

            if (existingAnswer is not null)
            {
                // Обновляем существующий ответ
                if (question.Type == QuestionType.MultipleChoice)
                {
                    existingAnswer.SetSelectedAnswerIds(answer.SelectedAnswerIds ?? new List<int>());
                    existingAnswer.TextAnswer = null;
                }
                else
                {
                    existingAnswer.SelectedAnswerIds = null;
                    existingAnswer.TextAnswer = answer.TextAnswer;
                }

                existingAnswer.UpdatedAt = DateTime.UtcNow;
                existingAnswer.IsCorrect = null; // Сбрасываем статус правильности, так как ответ изменился
            }
            else
            {
                // Создаем новый ответ
                var userAnswer = new UserAnswer
                {
                    UserId = userId,
                    QuizId = quiz.Id,
                    QuestionId = answer.QuestionId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (question.Type == QuestionType.MultipleChoice)
                {
                    userAnswer.SetSelectedAnswerIds(answer.SelectedAnswerIds ?? new List<int>());
                    userAnswer.TextAnswer = null;
                }
                else
                {
                    userAnswer.SelectedAnswerIds = null;
                    userAnswer.TextAnswer = answer.TextAnswer;
                }

                _context.UserAnswers.Add(userAnswer);
            }

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<List<QuestionAnswerDto>> GetUserAnswersAsync(string accessCode, string userId)
        {
            // Получаем ответы пользователя из базы данных
            var userAnswers = await _context.UserAnswers
                .Include(ua => ua.Question)
                .Where(ua => ua.Quiz.AccessCode == accessCode && ua.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            // Преобразуем в DTO
            return userAnswers.Select(ua => new QuestionAnswerDto
            {
                QuestionId = ua.QuestionId,
                SelectedAnswerIds = ua.GetSelectedAnswerIdsList(),
                TextAnswer = ua.TextAnswer
            }).ToList();
        }

        /// <inheritdoc />
        public async Task CompleteSessionAsync(string accessCode, string userId, QuizResultDto results)
        {
            // Получаем тест
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.AccessCode == accessCode);

            if (quiz == null)
            {
                throw new KeyNotFoundException($"Тест с кодом доступа {accessCode} не найден");
            }

            // Проверяем, есть ли уже результат
            var existingResult = await _context.QuizResults
                .FirstOrDefaultAsync(qr => qr.QuizId == quiz.Id && qr.UserId == userId);

            if (existingResult is not null)
            {
                // Обновляем существующий результат
                existingResult.CorrectAnswers = results.CorrectAnswers;
                existingResult.TotalQuestions = results.TotalQuestions;
                existingResult.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                // Создаем новый результат
                var quizResult = new Models.QuizResult
                {
                    UserId = userId,
                    QuizId = quiz.Id,
                    CorrectAnswers = results.CorrectAnswers,
                    TotalQuestions = results.TotalQuestions,
                    CompletedAt = DateTime.UtcNow
                };

                _context.QuizResults.Add(quizResult);
            }

            // Обновляем статусы правильности ответов
            foreach (var questionResult in results.Questions)
            {
                var userAnswer = await _context.UserAnswers
                    .FirstOrDefaultAsync(ua => 
                        ua.QuizId == quiz.Id && 
                        ua.QuestionId == questionResult.Id && 
                        ua.UserId == userId);

                if (userAnswer is not null)
                {
                    userAnswer.IsCorrect = questionResult.IsCorrect;
                    userAnswer.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<QuizResultDto?> GetSessionResultsAsync(string accessCode, string userId)
        {
            // Получаем тест
            var quiz = await _context.Quizzes
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.AccessCode == accessCode);

            if (quiz is null)
            {
                return null;
            }

            // Получаем результат теста
            var quizResult = await _context.QuizResults
                .AsNoTracking()
                .FirstOrDefaultAsync(qr => qr.QuizId == quiz.Id && qr.UserId == userId);

            if (quizResult is null)
            {
                return null;
            }

            // Получаем ответы пользователя
            var userAnswers = await _context.UserAnswers
                .Include(ua => ua.Question)
                .ThenInclude(q => q.Answers)
                .Where(ua => ua.QuizId == quiz.Id && ua.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            // Формируем результат
            return new QuizResultDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                TotalQuestions = quizResult.TotalQuestions,
                CorrectAnswers = quizResult.CorrectAnswers,
                Questions = await GetQuestionResultsFromUserAnswers(userAnswers)
            };
        }

        /// <inheritdoc />
        public async Task ClearSessionAsync(string accessCode, string userId)
        {
            // Получаем тест
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.AccessCode == accessCode);

            if (quiz == null)
            {
                return;
            }

            // Удаляем ответы пользователя
            var userAnswers = await _context.UserAnswers
                .Where(ua => ua.QuizId == quiz.Id && ua.UserId == userId)
                .ToListAsync();

            if (userAnswers.Any())
            {
                _context.UserAnswers.RemoveRange(userAnswers);
            }

            // Удаляем результат теста
            var quizResult = await _context.QuizResults
                .FirstOrDefaultAsync(qr => qr.QuizId == quiz.Id && qr.UserId == userId);

            if (quizResult != null)
            {
                _context.QuizResults.Remove(quizResult);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Создать результаты по вопросам на основе ответов пользователя
        /// </summary>
        private async Task<List<QuestionResultDto>> GetQuestionResultsFromUserAnswers(List<UserAnswer> userAnswers)
        {
            var result = new List<QuestionResultDto>();

            foreach (var userAnswer in userAnswers)
            {
                var question = userAnswer.Question;

                if (question.Type == QuestionType.MultipleChoice)
                {
                    // Для вопросов с выбором загружаем варианты ответов
                    var answerResults = await _context.Answers
                        .Where(a => a.QuestionId == question.Id)
                        .OrderBy(a => a.Order)
                        .Select(a => new AnswerResultDto
                        {
                            Id = a.Id,
                            Text = a.Text,
                            IsCorrect = a.IsCorrect,
                            IsSelected = userAnswer.GetSelectedAnswerIdsList().Contains(a.Id)
                        })
                        .AsNoTracking()
                        .ToListAsync();

                    result.Add(new QuestionResultDto
                    {
                        Id = question.Id,
                        Text = question.Text,
                        Type = question.Type,
                        Answers = answerResults,
                        IsCorrect = userAnswer.IsCorrect ?? false
                    });
                }
                else // FreeText
                {
                    result.Add(new QuestionResultDto
                    {
                        Id = question.Id,
                        Text = question.Text,
                        Type = question.Type,
                        UserAnswer = userAnswer.TextAnswer,
                        CorrectTextAnswer = question.CorrectTextAnswer,
                        IsCorrect = userAnswer.IsCorrect ?? false
                    });
                }
            }

            return result;
        }
    }
} 