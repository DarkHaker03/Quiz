# Система тестирования (Quiz System)

Система тестирования - это API для создания и прохождения тестов с разными типами вопросов.

## Функциональность

- Два типа вопросов: с выбором ответа и с развернутым ответом
- Хранение данных в PostgreSQL базе данных
- Сохранение ответов пользователя в базе данных для каждого прохождения теста
- Доступ к тестам по уникальной ссылке без необходимости авторизации
- Пошаговый процесс прохождения теста с сохранением ответов на каждом шаге
- Отслеживание ответов конкретного пользователя через идентификатор пользователя
- API для создания/редактирования тестов
- API для поэтапного прохождения тестов и получения результатов

## Требования

- .NET 8.0
- PostgreSQL

## Настройка и запуск

1. Установите [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Установите [PostgreSQL](https://www.postgresql.org/download/)
3. Обновите строку подключения в `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=QuizDb;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
   }
   ```
4. Запустите приложение:
   ```
   dotnet run
   ```
5. Откройте браузер и перейдите по адресу `https://localhost:5001` или `http://localhost:5000` для просмотра документации API

## API Endpoints

### Получение теста
`GET /api/Quiz/{accessCode}`

Получает тест по коду доступа.

### Создание теста
`POST /api/Quiz`

Создает новый тест. Пример тела запроса:
```json
{
  "title": "Название теста",
  "description": "Описание теста",
  "questions": [
    {
      "text": "Вопрос с выбором ответа",
      "type": 0,
      "order": 1,
      "answers": [
        {
          "text": "Вариант ответа 1",
          "isCorrect": true,
          "order": 1
        },
        {
          "text": "Вариант ответа 2",
          "isCorrect": false,
          "order": 2
        }
      ]
    },
    {
      "text": "Вопрос с развернутым ответом",
      "type": 1,
      "order": 2,
      "correctTextAnswer": "Правильный ответ"
    }
  ]
}
```

### Отправка ответа на вопрос
`POST /api/Quiz/answer?userId={userId}`

Отправляет ответ на отдельный вопрос. Требуется указать идентификатор пользователя в параметре запроса `userId`.

Пример тела запроса:
```json
{
  "accessCode": "AB12CD34",
  "answer": {
    "questionId": 1,
    "selectedAnswerIds": [1, 3]
  }
}
```

Для текстового вопроса:
```json
{
  "accessCode": "AB12CD34",
  "answer": {
    "questionId": 2,
    "textAnswer": "Текст ответа"
  }
}
```

### Получение сохраненных ответов пользователя
`GET /api/Quiz/{accessCode}/answers?userId={userId}`

Возвращает все ответы, которые пользователь уже отправил. Требуется указать идентификатор пользователя в параметре запроса `userId`.

### Завершение теста и получение результатов
`POST /api/Quiz/{accessCode}/submit?userId={userId}`

Завершает тест и возвращает результаты прохождения. Требуется указать идентификатор пользователя в параметре запроса `userId`.

## Структура базы данных

### Quiz (Тест)
- Id: int (Primary Key)
- Title: string
- Description: string
- AccessCode: string
- CreatedAt: DateTime

### Question (Вопрос)
- Id: int (Primary Key)
- QuizId: int (Foreign Key)
- Text: string
- Type: enum (MultipleChoice = 0, FreeText = 1)
- Order: int
- CorrectTextAnswer: string (для вопросов с развернутым ответом)

### Answer (Вариант ответа)
- Id: int (Primary Key)
- QuestionId: int (Foreign Key)
- Text: string
- IsCorrect: bool
- Order: int

### UserAnswer (Ответ пользователя)
- Id: int (Primary Key)
- UserId: string
- QuizId: int (Foreign Key)
- QuestionId: int (Foreign Key)
- SelectedAnswerIds: string (для вопросов с выбором, хранится в формате "1,2,3")
- TextAnswer: string (для вопросов с развернутым ответом)
- CreatedAt: DateTime
- UpdatedAt: DateTime
- IsCorrect: bool?

### QuizResult (Результат теста)
- Id: int (Primary Key)
- UserId: string
- QuizId: int (Foreign Key)
- TotalQuestions: int
- CorrectAnswers: int
- CompletedAt: DateTime

## Архитектура приложения

Приложение реализовано с использованием трехуровневой архитектуры:

1. **Слой контроллеров**: Обработка HTTP-запросов и ответов
2. **Слой сервисов**: Бизнес-логика приложения
3. **Слой данных**: Доступ к базе данных

### Сервисы
- **IQuizService**: Сервис для работы с тестами (создание, получение, проверка)
- **IUserSessionService**: Сервис для управления сессиями пользователей и сохранения их ответов в базе данных

### Идентификация пользователей
В этой системе каждый запрос, связанный с ответами пользователя, должен включать параметр `userId`, который идентифицирует пользователя. Этот идентификатор используется для сохранения ответов и результатов теста в базе данных. Клиентское приложение должно генерировать уникальный идентификатор для каждого пользователя и передавать его с каждым запросом. 