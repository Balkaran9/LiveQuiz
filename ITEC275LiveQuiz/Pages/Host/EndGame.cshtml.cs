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
}
