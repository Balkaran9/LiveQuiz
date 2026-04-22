using System.ComponentModel.DataAnnotations;
using ITEC275LiveQuiz.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Quizzes;

public class EditModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public int QuizId { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        var quiz = await dbContext.Quizzes.FirstOrDefaultAsync(q => q.QuizId == id && q.OwnerUserId == userId.Value);
        if (quiz is null)
        {
            return NotFound();
        }

        QuizId = quiz.QuizId;
        Input = new InputModel
        {
            Title = quiz.Title,
            IsPublic = quiz.IsPublic
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        if (!ModelState.IsValid)
        {
            QuizId = id;
            return Page();
        }

        var quiz = await dbContext.Quizzes.FirstOrDefaultAsync(q => q.QuizId == id && q.OwnerUserId == userId.Value);
        if (quiz is null)
        {
            return NotFound();
        }

        quiz.Title = Input.Title.Trim();
        quiz.IsPublic = Input.IsPublic;
        await dbContext.SaveChangesAsync();

        return RedirectToPage("Details", new { id = quiz.QuizId });
    }

    public class InputModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public bool IsPublic { get; set; }
    }
}
