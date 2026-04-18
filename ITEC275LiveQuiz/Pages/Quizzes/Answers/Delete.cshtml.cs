using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Quizzes.Answers;

public class DeleteModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public Answer? Answer { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        Answer = await dbContext.Answers
            .Include(a => a.Question)
            .ThenInclude(q => q!.Quiz)
            .FirstOrDefaultAsync(a => a.AnswerId == id && a.Question!.Quiz!.OwnerUserId == userId.Value);

        return Answer is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        var answer = await dbContext.Answers
            .Include(a => a.Question)
            .ThenInclude(q => q!.Quiz)
            .FirstOrDefaultAsync(a => a.AnswerId == id && a.Question!.Quiz!.OwnerUserId == userId.Value);

        if (answer is null)
        {
            return NotFound();
        }

        var questionId = answer.QuestionId;
        dbContext.Answers.Remove(answer);
        await dbContext.SaveChangesAsync();

        return RedirectToPage("/Quizzes/Questions/Edit", new { id = questionId });
    }
}
