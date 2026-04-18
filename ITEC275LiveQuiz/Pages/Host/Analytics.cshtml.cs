using ITEC275LiveQuiz.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Host;

public class AnalyticsModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public int LiveGameId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public List<QuestionAnalytics> QuestionStats { get; set; } = [];
    public List<ParticipantPerformance> TopPerformers { get; set; } = [];
    public double AverageAccuracy { get; set; }
    public double AverageResponseTime { get; set; }
    public string HardestQuestion { get; set; } = string.Empty;
    public string EasiestQuestion { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int gameId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return RedirectToLogin();

        var game = await dbContext.LiveGames
            .Include(g => g.Quiz)
            .FirstOrDefaultAsync(g => g.LiveGameId == gameId && g.HostUserId == userId.Value);

        if (game is null) return NotFound();

        LiveGameId = gameId;
        QuizTitle = game.Quiz?.Title ?? "Unknown";

        // Question-level analytics
        QuestionStats = await dbContext.LiveQuestions
            .Where(lq => lq.LiveGameId == gameId)
            .Include(lq => lq.Question)
            .Include(lq => lq.Responses)
            .Select(lq => new QuestionAnalytics
            {
                QuestionText = lq.Question!.QuestionText,
                TotalResponses = lq.Responses.Count,
                CorrectCount = lq.Responses.Count(r => r.IsCorrect),
                IncorrectCount = lq.Responses.Count(r => !r.IsCorrect),
                AverageTime = lq.Responses.Average(r => (double?)r.TimeMs) ?? 0,
                Accuracy = lq.Responses.Any() 
                    ? (double)lq.Responses.Count(r => r.IsCorrect) / lq.Responses.Count * 100 
                    : 0
            })
            .ToListAsync();

        // Top performers
        TopPerformers = await dbContext.LiveParticipants
            .Where(p => p.LiveGameId == gameId)
            .Select(p => new ParticipantPerformance
            {
                Nickname = p.Nickname,
                TotalPoints = p.Responses.Sum(r => r.PointsEarned),
                Accuracy = p.Responses.Any() 
                    ? (double)p.Responses.Count(r => r.IsCorrect) / p.Responses.Count * 100 
                    : 0,
                AverageTime = p.Responses.Average(r => (double?)r.TimeMs) ?? 0
            })
            .OrderByDescending(p => p.TotalPoints)
            .Take(10)
            .ToListAsync();

        // Overall stats
        if (QuestionStats.Any())
        {
            AverageAccuracy = QuestionStats.Average(q => q.Accuracy);
            AverageResponseTime = QuestionStats.Average(q => q.AverageTime) / 1000.0;
            
            var hardest = QuestionStats.OrderBy(q => q.Accuracy).FirstOrDefault();
            var easiest = QuestionStats.OrderByDescending(q => q.Accuracy).FirstOrDefault();
            
            HardestQuestion = hardest?.QuestionText ?? "N/A";
            EasiestQuestion = easiest?.QuestionText ?? "N/A";
        }

        return Page();
    }

    public class QuestionAnalytics
    {
        public string QuestionText { get; set; } = string.Empty;
        public int TotalResponses { get; set; }
        public int CorrectCount { get; set; }
        public int IncorrectCount { get; set; }
        public double AverageTime { get; set; }
        public double Accuracy { get; set; }
    }

    public class ParticipantPerformance
    {
        public string Nickname { get; set; } = string.Empty;
        public int TotalPoints { get; set; }
        public double Accuracy { get; set; }
        public double AverageTime { get; set; }
    }
}
