using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using ITEC275LiveQuiz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Host;

public class EndGameModel(AppDbContext dbContext, LeaderboardService leaderboardService) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public LiveGame? Game { get; set; }
    public List<LeaderboardEntry> Entries { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int gameId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        Game = await dbContext.LiveGames
            .Include(g => g.Quiz)
            .FirstOrDefaultAsync(g => g.LiveGameId == gameId && g.HostUserId == userId.Value);

        if (Game is null)
        {
            return NotFound();
        }

        if (Game.Status != "Ended")
        {
            // Close any open question first
            var openQuestion = await dbContext.LiveQuestions
                .FirstOrDefaultAsync(lq => lq.LiveGameId == gameId && lq.ClosedAt == null);
            if (openQuestion is not null)
            {
                openQuestion.ClosedAt = DateTime.UtcNow;
            }

            Game.Status = "Ended";
            Game.EndedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }

        Entries = await leaderboardService.GetLeaderboardAsync(gameId);
        return Page();
    }

    public async Task<IActionResult> OnGetExportAsync(int gameId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return RedirectToLogin();

        var game = await dbContext.LiveGames
            .Include(g => g.Quiz)
            .FirstOrDefaultAsync(g => g.LiveGameId == gameId && g.HostUserId == userId.Value);

        if (game is null) return NotFound();

        var entries = await leaderboardService.GetLeaderboardAsync(gameId);
        var csv = "Rank,Nickname,Score,Correct,Incorrect,AvgTime(s)\n";
        var rank = 1;
        foreach (var e in entries)
        {
            csv += $"{rank},{e.Nickname},{e.Score},{e.TotalCorrect},{e.TotalIncorrect},{Math.Round(e.AvgTimeMs / 1000.0, 1)}\n";
            rank++;
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"FinalResults-{game.Quiz?.Title}-{DateTime.Now:yyyyMMdd}.csv");
    }
}
