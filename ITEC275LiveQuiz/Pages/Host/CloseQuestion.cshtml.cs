using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Host;

public class CloseQuestionModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public LiveQuestion? LiveQuestion { get; set; }
    public List<ResponseResult> Results { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int liveQuestionId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        LiveQuestion = await dbContext.LiveQuestions
            .Include(lq => lq.LiveGame)
            .ThenInclude(g => g!.Quiz)
            .Include(lq => lq.Question)
            .FirstOrDefaultAsync(lq => lq.LiveQuestionId == liveQuestionId && lq.LiveGame!.HostUserId == userId.Value);

        if (LiveQuestion is null)
        {
            return NotFound();
        }

        if (LiveQuestion.ClosedAt is null)
        {
            LiveQuestion.ClosedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }

        Results = await dbContext.LiveResponses
            .AsNoTracking()
            .Where(r => r.LiveQuestionId == liveQuestionId)
            .Include(r => r.LiveParticipant)
            .Include(r => r.Answer)
            .OrderByDescending(r => r.IsCorrect)
            .ThenBy(r => r.TimeMs)
            .Select(r => new ResponseResult
            {
                Nickname = r.LiveParticipant!.Nickname,
                AnswerText = r.Answer!.AnswerText,
                IsCorrect = r.IsCorrect,
                TimeMs = r.TimeMs
            })
            .ToListAsync();

        return Page();
    }

    public class ResponseResult
    {
        public string Nickname { get; set; } = string.Empty;
        public string AnswerText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int TimeMs { get; set; }
    }
}
