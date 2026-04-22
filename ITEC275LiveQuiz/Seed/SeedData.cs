using BCrypt.Net;
using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Seed;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext dbContext)
    {
        if (await dbContext.Quizzes.AnyAsync())
        {
            return;
        }

        var demoUser = new User
        {
            Username = "demo",
            FullName = "Demo User",
            Email = "demo@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(demoUser);
        await dbContext.SaveChangesAsync();

        var quiz = new Quiz
        {
            OwnerUserId = demoUser.UserId,
            Title = "Demo Quiz - General Knowledge",
            IsPublic = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Quizzes.Add(quiz);
        await dbContext.SaveChangesAsync();

        var questions = new (string QuestionText, int TimeLimitSeconds, string[] Answers, int CorrectIndex)[]
        {
            ("What does CPU stand for?", 20, ["Central Processing Unit", "Computer Primary Unit", "Central Program Utility", "Core Processing User"], 0),
            ("Which company developed C#?", 20, ["Apple", "Microsoft", "Oracle", "Google"], 1),
            ("What HTML tag is used for the largest heading?", 20, ["<head>", "<heading>", "<h1>", "<title>"], 2),
            ("Which SQL statement is used to retrieve data?", 20, ["GET", "SELECT", "FETCH", "READ"], 1),
            ("What port does HTTPS use by default?", 20, ["80", "21", "443", "25"], 2),
            ("Which keyword creates a class instance in C#?", 20, ["make", "new", "create", "instance"], 1),
            ("What does CSS stand for?", 20, ["Creative Style Sheets", "Computer Style Syntax", "Cascading Style Sheets", "Colorful Style System"], 2),
            ("Which database is used in this project plan?", 20, ["MySQL", "SQL Server", "SQLite", "PostgreSQL"], 1),
            ("What symbol starts a Razor expression?", 20, ["#", "@", "$", "%"], 1),
            ("Which method saves EF Core changes?", 20, ["CommitAsync", "ApplyChanges", "SaveChangesAsync", "WriteAsync"], 2),
            ("What collection type stores multiple related entities?", 20, ["ICollection", "decimal", "DateTime", "Guid"], 0),
            ("Which ASP.NET Core feature stores per-user temporary data here?", 20, ["Cookies only", "Session", "SignalR", "Temp files"], 1),
            ("What does FK mean in database design?", 20, ["Fast Key", "Foreign Key", "File Key", "Fixed Key"], 1),
            ("Which LINQ method filters a sequence?", 20, ["Select", "OrderBy", "Where", "GroupBy"], 2),
            ("What is the default file extension for Razor Pages views?", 20, [".razor", ".cshtml", ".html", ".aspx"], 1)
        };

        var sortOrder = 1;
        foreach (var item in questions)
        {
            var question = new Question
            {
                QuizId = quiz.QuizId,
                QuestionText = item.QuestionText,
                TimeLimitSeconds = item.TimeLimitSeconds,
                SortOrder = sortOrder++
            };

            dbContext.Questions.Add(question);
            await dbContext.SaveChangesAsync();

            for (var i = 0; i < item.Answers.Length; i++)
            {
                dbContext.Answers.Add(new Answer
                {
                    QuestionId = question.QuestionId,
                    AnswerText = item.Answers[i],
                    IsCorrect = i == item.CorrectIndex
                });
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
