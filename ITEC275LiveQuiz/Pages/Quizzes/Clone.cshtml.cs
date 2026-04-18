using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Quizzes;

public class CloneModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return RedirectToLogin();

        var originalQuiz = await dbContext.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.QuizId == id && (q.OwnerUserId == userId.Value || q.IsPublic));

        if (originalQuiz is null) return NotFound();

        var clonedQuiz = new Quiz
        {
            OwnerUserId = userId.Value,
            Title = $"{originalQuiz.Title} (Copy)",
            IsPublic = false,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Quizzes.Add(clonedQuiz);
        await dbContext.SaveChangesAsync();

        foreach (var question in originalQuiz.Questions.OrderBy(q => q.SortOrder))
        {
            var clonedQuestion = new Question
            {
                QuizId = clonedQuiz.QuizId,
                QuestionText = question.QuestionText,
                TimeLimitSeconds = question.TimeLimitSeconds,
                SortOrder = question.SortOrder
            };

            dbContext.Questions.Add(clonedQuestion);
            await dbContext.SaveChangesAsync();

            foreach (var answer in question.Answers)
            {
                dbContext.Answers.Add(new Answer
                {
                    QuestionId = clonedQuestion.QuestionId,
                    AnswerText = answer.AnswerText,
                    IsCorrect = answer.IsCorrect
                });
            }
        }

        await dbContext.SaveChangesAsync();
        return RedirectToPage("Details", new { id = clonedQuiz.QuizId });
    }
}
