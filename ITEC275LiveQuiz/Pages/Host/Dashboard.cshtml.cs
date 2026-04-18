using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Host;

public class DashboardModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public int TotalQuizzes { get; set; }
    public int TotalGames { get; set; }
    public int TotalParticipants { get; set; }
    public int ActiveGames { get; set; }
    public List<RecentGame> RecentGames { get; set; } = [];
    public List<LiveGame> PastGames { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return RedirectToLogin();

        TotalQuizzes = await dbContext.Quizzes.CountAsync(q => q.OwnerUserId == userId.Value);
        TotalGames = await dbContext.LiveGames.CountAsync(g => g.HostUserId == userId.Value);
        TotalParticipants = await dbContext.LiveGames
            .Where(g => g.HostUserId == userId.Value)
            .SelectMany(g => g.Participants)
            .CountAsync();
        ActiveGames = await dbContext.LiveGames
            .CountAsync(g => g.HostUserId == userId.Value && g.Status != "Ended");

        RecentGames = await dbContext.LiveGames
            .Where(g => g.HostUserId == userId.Value)
            .Include(g => g.Quiz)
            .OrderByDescending(g => g.StartedAt)
            .Take(5)
            .Select(g => new RecentGame
            {
                LiveGameId = g.LiveGameId,
                QuizTitle = g.Quiz!.Title,
                StartedAt = g.StartedAt,
                Status = g.Status,
                ParticipantCount = g.Participants.Count
            })
            .ToListAsync();

        PastGames = await dbContext.LiveGames
            .Where(g => g.HostUserId == userId.Value)
            .Include(g => g.Quiz)
            .OrderByDescending(g => g.StartedAt)
            .ToListAsync();

        return Page();
    }

    public class RecentGame
    {
        public int LiveGameId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ParticipantCount { get; set; }
    }
}
