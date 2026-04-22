using System.ComponentModel.DataAnnotations;
using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;

namespace ITEC275LiveQuiz.Pages.Quizzes;

public class CreateModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IActionResult OnGet()
    {
        return GetCurrentUserId().HasValue ? Page() : RedirectToLogin();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var quiz = new Quiz
            {
                OwnerUserId = userId.Value,
                Title = Input.Title.Trim(),
                Category = null,
                IsPublic = Input.IsPublic,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Quizzes.Add(quiz);
            await dbContext.SaveChangesAsync();

            return RedirectToPage("Details", new { id = quiz.QuizId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Unable to create quiz: {ex.Message}");
            return Page();
        }
    }

    public class InputModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public bool IsPublic { get; set; }
    }
}
