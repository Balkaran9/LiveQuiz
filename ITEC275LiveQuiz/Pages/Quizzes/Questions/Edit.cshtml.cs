using System.ComponentModel.DataAnnotations;
using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Quizzes.Questions;

public class EditModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Question? Question { get; set; }
    public List<Answer> Answers { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        return await LoadAsync(id);
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

        if (!ModelState.IsValid)
        {
            return await LoadAsync(id);
        }

        question.QuestionText = Input.QuestionText.Trim();
        question.TimeLimitSeconds = Input.TimeLimitSeconds;
        question.SortOrder = Input.SortOrder;
        await dbContext.SaveChangesAsync();

        return RedirectToPage("/Quizzes/Details", new { id = question.QuizId });
    }

    private async Task<IActionResult> LoadAsync(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        Question = await dbContext.Questions
            .Include(q => q.Quiz)
            .FirstOrDefaultAsync(q => q.QuestionId == id && q.Quiz!.OwnerUserId == userId.Value);

        if (Question is null)
        {
            return NotFound();
        }

        Answers = await dbContext.Answers
            .Where(a => a.QuestionId == id)
            .OrderBy(a => a.AnswerId)
            .ToListAsync();

        Input = new InputModel
        {
            QuestionText = Question.QuestionText,
            TimeLimitSeconds = Question.TimeLimitSeconds,
            SortOrder = Question.SortOrder
        };

        return Page();
    }

    public class InputModel
    {
        [Required]
        [StringLength(500)]
        public string QuestionText { get; set; } = string.Empty;

        [Range(5, 300)]
        public int TimeLimitSeconds { get; set; }

        public int SortOrder { get; set; }
    }
}
