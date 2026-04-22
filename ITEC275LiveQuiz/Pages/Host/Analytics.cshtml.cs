using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ITEC275LiveQuiz.Pages.Host;

public class AnalyticsModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public LiveGame? Game { get; set; }
    public List<QuestionAnalytics> QuestionStats { get; set; } = [];
    public GameAnalytics GameStats { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int gameId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return RedirectToLogin();

        Game = await dbContext.LiveGames
            .AsNoTracking()
            .Include(g => g.Quiz)
            .Include(g => g.Participants)
            .Include(g => g.LiveQuestions)
            .ThenInclude(lq => lq.Question)
            .FirstOrDefaultAsync(g => g.LiveGameId == gameId && g.HostUserId == userId.Value);

        if (Game is null) return NotFound();

        // Calculate question statistics
        foreach (var liveQuestion in Game.LiveQuestions.OrderBy(lq => lq.Question!.SortOrder))
        {
            var responses = await dbContext.LiveResponses
                .AsNoTracking()
                .Where(r => r.LiveQuestionId == liveQuestion.LiveQuestionId)
                .ToListAsync();

            var correctCount = responses.Count(r => r.IsCorrect);
            var totalCount = responses.Count;
            
            QuestionStats.Add(new QuestionAnalytics
            {
                QuestionText = liveQuestion.Question?.QuestionText ?? "",
                TotalResponses = totalCount,
                CorrectResponses = correctCount,
                IncorrectResponses = totalCount - correctCount,
                CorrectPercentage = totalCount > 0 ? (correctCount * 100.0 / totalCount) : 0,
                AvgTimeSeconds = responses.Any() ? responses.Average(r => r.TimeMs) / 1000.0 : 0
            });
        }

        // Calculate overall game statistics
        var allResponses = await dbContext.LiveResponses
            .AsNoTracking()
            .Where(r => Game.LiveQuestions.Select(lq => lq.LiveQuestionId).Contains(r.LiveQuestionId))
            .ToListAsync();

        GameStats = new GameAnalytics
        {
            TotalParticipants = Game.Participants.Count,
            TotalQuestions = Game.LiveQuestions.Count,
            TotalResponses = allResponses.Count,
            CorrectResponses = allResponses.Count(r => r.IsCorrect),
            IncorrectResponses = allResponses.Count(r => !r.IsCorrect),
            AvgAccuracy = allResponses.Any() ? (allResponses.Count(r => r.IsCorrect) * 100.0 / allResponses.Count) : 0,
            AvgResponseTime = allResponses.Any() ? allResponses.Average(r => r.TimeMs) / 1000.0 : 0
        };

        return Page();
    }

    public async Task<IActionResult> OnGetExportCsvAsync(int gameId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return RedirectToLogin();

        var game = await dbContext.LiveGames
            .AsNoTracking()
            .Include(g => g.Quiz)
            .Include(g => g.LiveQuestions)
            .ThenInclude(lq => lq.Question)
            .FirstOrDefaultAsync(g => g.LiveGameId == gameId && g.HostUserId == userId.Value);

        if (game is null) return NotFound();

        var responses = await dbContext.LiveResponses
            .AsNoTracking()
            .Where(r => game.LiveQuestions.Select(lq => lq.LiveQuestionId).Contains(r.LiveQuestionId))
            .Include(r => r.LiveParticipant)
            .Include(r => r.LiveQuestion)
            .ThenInclude(lq => lq!.Question)
            .Include(r => r.Answer)
            .OrderBy(r => r.LiveParticipant!.Nickname)
            .ThenBy(r => r.LiveQuestion!.Question!.SortOrder)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Participant,Question,Answer,Correct,Time (seconds)");

        foreach (var response in responses)
        {
            csv.AppendLine($"\"{response.LiveParticipant?.Nickname}\",\"{response.LiveQuestion?.Question?.QuestionText}\",\"{response.Answer?.AnswerText}\",{response.IsCorrect},{response.TimeMs / 1000.0:F1}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"LiveQuiz_{game.Quiz?.Title}_Analytics_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    public class QuestionAnalytics
    {
        public string QuestionText { get; set; } = string.Empty;
        public int TotalResponses { get; set; }
        public int CorrectResponses { get; set; }
        public int IncorrectResponses { get; set; }
        public double CorrectPercentage { get; set; }
        public double AvgTimeSeconds { get; set; }
    }

    public class GameAnalytics
    {
        public int TotalParticipants { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalResponses { get; set; }
        public int CorrectResponses { get; set; }
        public int IncorrectResponses { get; set; }
        public double AvgAccuracy { get; set; }
        public double AvgResponseTime { get; set; }
    }
}
