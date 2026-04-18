using System.ComponentModel.DataAnnotations;
using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Quizzes.Answers;

public class CreateModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Question? Question { get; set; }

    public async Task<IActionResult> OnGetAsync(int questionId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        Question = await dbContext.Questions
            .Include(q => q.Quiz)
            .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.Quiz!.OwnerUserId == userId.Value);

        return Question is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int questionId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        Question = await dbContext.Questions
            .Include(q => q.Quiz)
            .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.Quiz!.OwnerUserId == userId.Value);

        if (Question is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.IsCorrect)
        {
            var existingAnswers = await dbContext.Answers.Where(a => a.QuestionId == questionId).ToListAsync();
            foreach (var existingAnswer in existingAnswers)
            {
                existingAnswer.IsCorrect = false;
            }
        }

        dbContext.Answers.Add(new Answer
        {
            QuestionId = questionId,
            AnswerText = Input.AnswerText.Trim(),
            IsCorrect = Input.IsCorrect
        });

        await dbContext.SaveChangesAsync();
        return RedirectToPage("/Quizzes/Questions/Edit", new { id = questionId });
    }

    public class InputModel
    {
        [Required]
        [StringLength(250)]
        public string AnswerText { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
    }
}
