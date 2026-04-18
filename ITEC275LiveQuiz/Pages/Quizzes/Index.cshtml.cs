using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Quizzes;

public class IndexModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public List<Quiz> Quizzes { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        var query = dbContext.Quizzes
            .Where(q => q.OwnerUserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(q => q.Title.Contains(SearchTerm));
        }

        Quizzes = await query
            .Include(q => q.Questions)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

        return Page();
    }
}
