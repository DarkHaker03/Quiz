using Microsoft.EntityFrameworkCore;
using Quiz.Models;

namespace Quiz.Data
{
    /// <summary>
    /// Контекст базы данных для системы викторин
    /// </summary>
    public class QuizDbContext : DbContext
    {
        public QuizDbContext(DbContextOptions<QuizDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Models.Quiz> Quizzes { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<Answer> Answers { get; set; } = null!;
        public DbSet<UserAnswer> UserAnswers { get; set; } = null!;
        public DbSet<QuizResult> QuizResults { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка PostgreSQL конфигурации для таблиц
            modelBuilder.Entity<Models.Quiz>().ToTable("quizzes");
            modelBuilder.Entity<Question>().ToTable("questions");
            modelBuilder.Entity<Answer>().ToTable("answers");
            modelBuilder.Entity<UserAnswer>().ToTable("user_answers");
            modelBuilder.Entity<QuizResult>().ToTable("quiz_results");

            // Настройка каскадного удаления
            modelBuilder.Entity<Models.Quiz>()
                .HasMany(q => q.Questions)
                .WithOne(q => q.Quiz)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка связей для ответов пользователя
            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Quiz)
                .WithMany()
                .HasForeignKey(ua => ua.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Question)
                .WithMany()
                .HasForeignKey(ua => ua.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка связей для результатов теста
            modelBuilder.Entity<QuizResult>()
                .HasOne(qr => qr.Quiz)
                .WithMany()
                .HasForeignKey(qr => qr.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // Индексы для оптимизации запросов
            modelBuilder.Entity<UserAnswer>()
                .HasIndex(ua => new { ua.UserId, ua.QuizId, ua.QuestionId })
                .IsUnique();

            modelBuilder.Entity<QuizResult>()
                .HasIndex(qr => new { qr.UserId, qr.QuizId })
                .IsUnique();
        }
    }
} 