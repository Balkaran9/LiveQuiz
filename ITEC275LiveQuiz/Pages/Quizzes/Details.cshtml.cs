using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Quizzes;

public class DetailsModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public Quiz? Quiz { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        Quiz = await dbContext.Quizzes
            .Include(q => q.Questions.OrderBy(qn => qn.SortOrder))
            .ThenInclude(qn => qn.Answers.OrderBy(a => a.AnswerId))
            .FirstOrDefaultAsync(q => q.QuizId == id && q.OwnerUserId == userId.Value);

        if (Quiz is null)
        {
            return NotFound();
        }

        return Page();
    }
}
