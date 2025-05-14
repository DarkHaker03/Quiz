using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Quiz.Data;
using Quiz.Services;
// using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контекст базы данных с PostgreSQL
builder.Services.AddDbContext<QuizDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрируем сервисы
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddScoped<IQuizService, QuizService>();

// Добавляем контроллеры
builder.Services.AddControllers();

// Настраиваем CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Добавляем Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Quiz API",
        Version = "v1",
        Description = "API для системы тестирования с вопросами разных типов",
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();
app.UseAuthorization();

app.MapControllers();

// Создаем базу данных при запуске, если её нет
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<QuizDbContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception)
    {
        // Игнорируем ошибки при создании базы данных
    }
}

app.Run();
