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
            Title = "Hilarious World History",
            Category = "History",
            IsPublic = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Quizzes.Add(quiz);
        await dbContext.SaveChangesAsync();

        var questions = new (string QuestionText, int TimeLimitSeconds, string[] Answers, int CorrectIndex)[]
        {
            ("What did Napoleon Bonaparte ACTUALLY fear the most?", 20, ["Heights", "Cats", "Water", "Thunder"], 1),
            ("Which ancient civilization invented the first vending machine?", 20, ["Romans", "Greeks", "Egyptians", "Chinese"], 1),
            ("What bizarre item did the ancient Egyptians use as currency?", 20, ["Teeth", "Onions", "Stones", "Feathers"], 1),
            ("Who was the shortest serving British Prime Minister ever?", 20, ["Lasted 2 days", "Lasted 119 days", "Lasted 1 hour", "Never took office"], 1),
            ("What did Albert Einstein's last words go unrecorded?", 20, ["Nurse didn't speak German", "He whispered", "He was asleep", "He forgot"], 0),
            ("In 1919, what strange disaster killed 21 people in Boston?", 20, ["Molasses flood", "Cheese avalanche", "Milk tsunami", "Butter explosion"], 0),
            ("What did ancient Roman gladiators endorse?", 20, ["Chariots", "Olive oil like athletes", "Wine", "Sandals"], 1),
            ("Who once declared war on Neptune (the sea)?", 20, ["Julius Caesar", "Caligula", "Nero", "Augustus"], 1),
            ("What did Thomas Edison's last breath get preserved in?", 20, ["A test tube by Henry Ford", "A bottle", "A balloon", "Nothing"], 0),
            ("In medieval times, what did people believe caused the Black Plague?", 20, ["Bad smells", "Cats", "The moon", "Loud noises"], 0),
            ("What animal did Andrew Jackson keep in the White House?", 20, ["Alligator", "Lion", "Bear", "Elephant"], 0),
            ("What did Vikings use as primitive sunglasses?", 20, ["Walrus ivory", "Wood", "Leather", "Nothing"], 0),
            ("Who sold the Eiffel Tower twice to scrap metal dealers?", 20, ["Victor Lustig", "Gustave Eiffel", "Napoleon III", "Charles de Gaulle"], 0),
            ("What did ancient Assyrians believe caused headaches?", 20, ["Evil spirits", "Bad food", "Lack of sleep", "Stress"], 0),
            ("What did Cleopatra reportedly bathe in to stay beautiful?", 20, ["Milk and honey", "Wine", "Rose water", "Olive oil"], 0)
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
