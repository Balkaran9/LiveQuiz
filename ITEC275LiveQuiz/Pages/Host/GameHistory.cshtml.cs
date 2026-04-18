using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Host;

public class GameHistoryModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public List<GameHistoryEntry> Games { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return RedirectToLogin();

        Games = await dbContext.LiveGames
            .Where(g => g.HostUserId == userId.Value)
            .Include(g => g.Quiz)
            .Include(g => g.Participants)
            .Include(g => g.LiveQuestions)
            .OrderByDescending(g => g.StartedAt)
            .Select(g => new GameHistoryEntry
            {
                LiveGameId = g.LiveGameId,
                QuizTitle = g.Quiz!.Title,
                JoinCode = g.JoinCode,
                StartedAt = g.StartedAt,
                EndedAt = g.EndedAt,
                Status = g.Status,
                ParticipantCount = g.Participants.Count,
                QuestionCount = g.LiveQuestions.Count
            })
            .ToListAsync();

        return Page();
    }

    public class GameHistoryEntry
    {
        public int LiveGameId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ParticipantCount { get; set; }
        public int QuestionCount { get; set; }
    }
}
