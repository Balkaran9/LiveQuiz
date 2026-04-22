using System.ComponentModel.DataAnnotations;
using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Quizzes.Questions;

public class CreateModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int quizId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        var quiz = await dbContext.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId && q.OwnerUserId == userId.Value);
        if (quiz is null)
        {
            return NotFound();
        }

        QuizId = quiz.QuizId;
        QuizTitle = quiz.Title;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int quizId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        var quiz = await dbContext.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId && q.OwnerUserId == userId.Value);
        if (quiz is null)
        {
            return NotFound();
        }

        QuizId = quiz.QuizId;
        QuizTitle = quiz.Title;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var question = new Question
        {
            QuizId = quizId,
            QuestionText = Input.QuestionText.Trim(),
            TimeLimitSeconds = Input.TimeLimitSeconds,
            SortOrder = Input.SortOrder
        };

        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync();

        return RedirectToPage("/Quizzes/Details", new { id = quizId });
    }

    public class InputModel
    {
        [Required]
        [StringLength(500)]
        public string QuestionText { get; set; } = string.Empty;

        [Range(5, 300)]
        public int TimeLimitSeconds { get; set; } = 20;

        public int SortOrder { get; set; } = 1;
    }
}
