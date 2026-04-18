using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Quizzes.Questions;

public class DeleteModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public Question? Question { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        Question = await dbContext.Questions
            .Include(q => q.Quiz)
            .FirstOrDefaultAsync(q => q.QuestionId == id && q.Quiz!.OwnerUserId == userId.Value);

        return Question is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        var question = await dbContext.Questions
            .Include(q => q.Quiz)
            .FirstOrDefaultAsync(q => q.QuestionId == id && q.Quiz!.OwnerUserId == userId.Value);

        if (question is null)
        {
            return NotFound();
        }

        dbContext.Questions.Remove(question);
        await dbContext.SaveChangesAsync();
        return RedirectToPage("/Quizzes/Details", new { id = question.QuizId });
    }
}
