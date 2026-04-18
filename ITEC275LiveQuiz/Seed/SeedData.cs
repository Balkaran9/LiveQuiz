using BCrypt.Net;
using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Seed;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext dbContext)
    {
        // If there are no quizzes, seed demo data
        if (!await dbContext.Quizzes.AnyAsync())
        {
            var demoUser = new User
            {
                Username = "demo",
                Email = "demo@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(demoUser);
            await dbContext.SaveChangesAsync();

            var quiz = new Quiz
            {
                Title = "Demo Quiz",
                Category = "General Knowledge",
                IsPublic = true,
                OwnerUserId = demoUser.UserId,
                CreatedAt = DateTime.UtcNow,
                Questions = new List<Question>()
            };
            dbContext.Quizzes.Add(quiz);
            await dbContext.SaveChangesAsync();

            // Add 5 demo questions
            for (int i = 1; i <= 5; i++)
            {
                var q = new Question
                {
                    QuizId = quiz.QuizId,
                    QuestionText = $"Sample Question {i}?",
                    Answers = new List<Answer>()
                };
                dbContext.Questions.Add(q);
                await dbContext.SaveChangesAsync();

                for (int j = 1; j <= 4; j++)
                {
                    dbContext.Answers.Add(new Answer
                    {
                        QuestionId = q.QuestionId,
                        AnswerText = $"Option {j}",
                        IsCorrect = (j == 1)
                    });
                }
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
