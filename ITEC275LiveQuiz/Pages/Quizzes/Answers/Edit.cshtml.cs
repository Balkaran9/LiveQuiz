using System.ComponentModel.DataAnnotations;
using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Quizzes.Answers;

public class EditModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Answer? Answer { get; set; }
    public string QuestionText { get; set; } = string.Empty;

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

        var answer = await dbContext.Answers
            .Include(a => a.Question)
            .ThenInclude(q => q!.Quiz)
            .FirstOrDefaultAsync(a => a.AnswerId == id && a.Question!.Quiz!.OwnerUserId == userId.Value);

        if (answer is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return await LoadAsync(id);
        }

        if (Input.IsCorrect)
        {
            var siblings = await dbContext.Answers.Where(a => a.QuestionId == answer.QuestionId && a.AnswerId != id).ToListAsync();
            foreach (var sibling in siblings)
            {
                sibling.IsCorrect = false;
            }
        }

        answer.AnswerText = Input.AnswerText.Trim();
        answer.IsCorrect = Input.IsCorrect;
        await dbContext.SaveChangesAsync();

        return RedirectToPage("/Quizzes/Questions/Edit", new { id = answer.QuestionId });
    }

    private async Task<IActionResult> LoadAsync(int id)
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

        if (Answer is null)
        {
            return NotFound();
        }

        QuestionText = Answer.Question!.QuestionText;
        Input = new InputModel
        {
            AnswerText = Answer.AnswerText,
            IsCorrect = Answer.IsCorrect
        };

        return Page();
    }

    public class InputModel
    {
        [Required]
        [StringLength(250)]
        public string AnswerText { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
    }
}
