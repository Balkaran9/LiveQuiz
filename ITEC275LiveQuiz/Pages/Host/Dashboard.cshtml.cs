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
            var statsTask = dbContext.Quizzes
                .AsNoTracking()
                .Where(q => q.OwnerUserId == userId.Value)
                .CountAsync();
            
            var gamesTask = dbContext.LiveGames
                .AsNoTracking()
                .Where(g => g.HostUserId == userId.Value)
                .Select(g => new 
                { 
                    g.LiveGameId, 
                    g.Status, 
                    ParticipantCount = g.Participants.Count 
                })
                .ToListAsync();

            var recentGamesTask = dbContext.LiveGames
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

            await Task.WhenAll(statsTask, gamesTask, recentGamesTask);

            TotalQuizzes = await statsTask;
            var games = await gamesTask;
            TotalGames = games.Count;
            TotalParticipants = games.Sum(g => g.ParticipantCount);
            ActiveGames = games.Count(g => g.Status != "Ended");
            RecentGames = await recentGamesTask;

            return Page();
        }
        catch (Exception)
        {
            // If there's any database error, return with default values
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
