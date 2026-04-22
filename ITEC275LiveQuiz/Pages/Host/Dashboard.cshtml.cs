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

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return RedirectToLogin();

        try
        {
            TotalQuizzes = await dbContext.Quizzes
                .AsNoTracking()
                .Where(q => q.OwnerUserId == userId.Value)
                .CountAsync();
            
            var games = await dbContext.LiveGames
                .AsNoTracking()
                .Where(g => g.HostUserId == userId.Value)
                .Include(g => g.Participants)
                .ToListAsync();

            TotalGames = games.Count;
            TotalParticipants = games.Sum(g => g.Participants.Count);
            ActiveGames = games.Count(g => g.Status != "Ended");

            RecentGames = await dbContext.LiveGames
                .AsNoTracking()
                .Where(g => g.HostUserId == userId.Value)
                .Include(g => g.Quiz)
                .Include(g => g.Participants)
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

            return Page();
        }
        catch (Exception)
        {
            TotalQuizzes = 0;
            TotalGames = 0;
            TotalParticipants = 0;
            ActiveGames = 0;
            RecentGames = new List<RecentGame>();
            return Page();
        }
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
