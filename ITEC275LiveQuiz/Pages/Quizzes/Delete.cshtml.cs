using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Quizzes;

public class DeleteModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public Quiz? Quiz { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return RedirectToLogin();

        Quiz = await dbContext.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.QuizId == id && q.OwnerUserId == userId.Value);

        return Quiz is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return RedirectToLogin();

        var quiz = await dbContext.Quizzes
            .FirstOrDefaultAsync(q => q.QuizId == id && q.OwnerUserId == userId.Value);

        if (quiz is null) return NotFound();

        dbContext.Quizzes.Remove(quiz);
        await dbContext.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
