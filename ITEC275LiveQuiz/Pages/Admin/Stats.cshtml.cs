using ITEC275LiveQuiz.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Admin;

public class StatsModel(AppDbContext dbContext, ILogger<StatsModel> logger) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public PlatformStats Stats { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        logger.LogInformation("Loading platform statistics");

        try
        {
            Stats = new PlatformStats
            {
                TotalUsers = await dbContext.Users.CountAsync(),
                TotalQuizzes = await dbContext.Quizzes.CountAsync(),
                PublicQuizzes = await dbContext.Quizzes.CountAsync(q => q.IsPublic),
                TotalGames = await dbContext.LiveGames.CountAsync(),
                CompletedGames = await dbContext.LiveGames.CountAsync(g => g.Status == "Ended"),
                ActiveGames = await dbContext.LiveGames.CountAsync(g => g.Status == "Lobby" || g.Status == "InProgress"),
                TotalParticipants = await dbContext.LiveParticipants.CountAsync(),
                TotalQuestions = await dbContext.Questions.CountAsync(),
                TotalResponses = await dbContext.LiveResponses.CountAsync(),
                AverageQuestionsPerQuiz = await dbContext.Quizzes
                    .Where(q => q.Questions!.Any())
                    .AverageAsync(q => (double?)q.Questions!.Count) ?? 0,
                AverageParticipantsPerGame = await dbContext.LiveGames
                    .Where(g => g.Participants!.Any())
                    .AverageAsync(g => (double?)g.Participants!.Count) ?? 0,
                MostPopularCategory = await GetMostPopularCategoryAsync(),
                TopQuizCreator = await GetTopQuizCreatorAsync(),
                RecentActivity = await GetRecentActivityAsync()
            };

            logger.LogInformation("Statistics loaded successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading statistics");
            ModelState.AddModelError(string.Empty, "Error loading statistics");
        }

        return Page();
    }

    private async Task<string> GetMostPopularCategoryAsync()
    {
        try
        {
            var categoryGroup = await dbContext.Quizzes
                .Where(q => !string.IsNullOrEmpty(q.Category))
                .GroupBy(q => q.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            return categoryGroup?.Category ?? "N/A";
        }
        catch
        {
            return "N/A";
        }
    }

    private async Task<string> GetTopQuizCreatorAsync()
    {
        try
        {
            var topCreator = await dbContext.Users
                .Include(u => u.OwnedQuizzes)
                .OrderByDescending(u => u.OwnedQuizzes!.Count)
                .FirstOrDefaultAsync();

            return topCreator?.Username ?? "N/A";
        }
        catch
        {
            return "N/A";
        }
    }

    private async Task<List<ActivityItem>> GetRecentActivityAsync()
    {
        try
        {
            var recentGames = await dbContext.LiveGames
                .Include(g => g.Quiz)
                .OrderByDescending(g => g.StartedAt)
                .Take(10)
                .Select(g => new ActivityItem
                {
                    Type = "Game",
                    Description = $"Game '{g.Quiz!.Title}' - {g.Status}",
                    Timestamp = g.StartedAt
                })
                .ToListAsync();

            return recentGames;
        }
        catch
        {
            return new List<ActivityItem>();
        }
    }

    public class PlatformStats
    {
        public int TotalUsers { get; set; }
        public int TotalQuizzes { get; set; }
        public int PublicQuizzes { get; set; }
        public int TotalGames { get; set; }
        public int CompletedGames { get; set; }
        public int ActiveGames { get; set; }
        public int TotalParticipants { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalResponses { get; set; }
        public double AverageQuestionsPerQuiz { get; set; }
        public double AverageParticipantsPerGame { get; set; }
        public string MostPopularCategory { get; set; } = string.Empty;
        public string TopQuizCreator { get; set; } = string.Empty;
        public List<ActivityItem> RecentActivity { get; set; } = [];
    }

    public class ActivityItem
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
